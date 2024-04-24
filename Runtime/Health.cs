using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
	public class HealthEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Value of health
		/// </summary>
		public float value { get; set; }

		/// <summary>
		/// Normalized value of health
		/// </summary>
		public float normalizedValue { get; set; }

		/// <summary>
		/// Amount of health change
		/// </summary>
		public float delta { get; set; }

		/// <summary>
		/// Factor applied before
		/// </summary>
		public float preDamageFactor { get; set; } = 1f;

		/// <summary>
		/// Factor applied after
		/// </summary>
		public float postDamageFactor { get; set; } = 1f;

		public DamageHit hit { get; private set; }

		public GameObject source { get; private set; }

		#endregion

		#region Constructors

		public HealthEventArgs()
		{ }

		public HealthEventArgs(DamageHit hit, GameObject source = null)
		{
			this.hit = hit;
			this.source = source;
		}

		public HealthEventArgs(HealthEventArgs copy)
		{
			value = copy.value;
			normalizedValue = copy.normalizedValue;
			delta = copy.delta;
			preDamageFactor = copy.preDamageFactor;
			postDamageFactor = copy.postDamageFactor;
			hit = new DamageHit(copy.hit);
			source = copy.source;
		}

		#endregion

		#region Methods

		public float GetDelta()
		{
			return IHealthModifier.GetDelta(
				// Pre-damage factor cannot be less than 0 (cannot heal from faction and/or difficulty scaling)
				// But negative value still needs to be considered earlier for proper modifications
				IHealthModifier.GetDelta(hit.modifiedValue, Mathf.Max(0f, preDamageFactor)),
				postDamageFactor);
		}

		#endregion
	}

	public class Health : MonoBehaviour, IHealth, IHealthRegeneration, IPoolItemRecyclable
	{
		#region Enumerators

		[Flags]
		public enum RegenerateStop
		{
			Zero = 1 << 1,
			Maximum = 1 << 2,
		}

		#endregion

		#region Fields

		[SerializeField, Min(0f)]
		private float m_value = 0f;

		[SerializeField]
		private float m_bonusValue = 0f;

		[SerializeField, Min(0f)]
		private float m_maxValue = 0f;

		[SerializeField, Min(0f), Tooltip("Seconds of invulnerability after receiving damage.")]
		private float m_invulnerabilityTime;

		[SerializeField, Tooltip("Indicates whether health cannot initially receive damage.")]
		private bool m_startInvulnerable;

		/// <summary>
		/// Seconds before regeneration begins
		/// </summary>
		[SerializeField, Min(0f), Tooltip("Seconds before regeneration begins (only used for NULL damage type with positive rate).")]
		private float m_regenerateDelay;

		[SerializeField, Min(0f), Tooltip("Seconds before degeneration begins (only used for NULL damage type with negative rate).")]
		private float m_degenerateDelay;

		[SerializeField, Tooltip("Health regenerated (or degenerated) per second by damage type.")]
		private List<Damage> m_rates = new();

		[SerializeField, Tooltip("Indicates whether regeneration can occur while dead.")]
		private RegenerateStop m_stopCondition = RegenerateStop.Zero | RegenerateStop.Maximum;

		private bool m_isInvulnerable = false;
		private HashSet<Component> m_externalInvulnerabilities = new();
		private Coroutine m_invulnerabilityThread = null;

		private DamageHit m_nullRegenerateRate = null;
		private Dictionary<DamageType, DamageHit> m_regenerateRates = new();
		private Coroutine m_regenerateThread = null;
		private float m_remainingDelayTime = 0f;

		#endregion

		#region Events

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onValueChanging = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onValueChanged = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onHealed = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDamaged = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDying = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDied = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onResurrected = new UnityEvent<HealthEventArgs>();

		public UnityEvent OnRegenerationChanged = new UnityEvent();
		public event EventHandler Killed;

		#endregion

		#region Properties

		public float value
		{
			get => m_value;
			set => m_value = value;
		}

		public float bonusValue
		{
			get => m_bonusValue;
			set
			{
				// No change, skip
				if (m_bonusValue == value)
					return;

				float delta = value - m_bonusValue;
				m_bonusValue = value;

				m_value = Mathf.Clamp(m_value + delta, 0, maxValue);

				var eventArgs = new HealthEventArgs()
				{
					value = m_value,
					normalizedValue = m_value / maxValue,
					delta = delta,
				};
				onValueChanged.Invoke(eventArgs);

				if (isDead)
				{
					this.CancelCoroutine(ref m_invulnerabilityThread);

					if ((m_stopCondition & RegenerateStop.Zero) != 0)
					{
						this.CancelCoroutine(ref m_regenerateThread);
					}
					onDied.Invoke(eventArgs);
				}
			}
		}

		public float maxValue => m_maxValue + m_bonusValue;

		public float normalizedValue => m_value / maxValue;

		public bool canRegenerate => m_nullRegenerateRate != null || m_regenerateRates.Count > 0;

		public float regenerateDelay { get => m_regenerateDelay; set => m_regenerateDelay = value; }
		public float degenerateDelay { get => m_degenerateDelay; set => m_degenerateDelay = value; }

		public DamageType[] regenerateDamageTypes
		{
			get
			{
				List<DamageType> types = new();
				if (m_nullRegenerateRate != null)
				{
					types.Add(null);
				}

				types.AddRange(m_regenerateRates.Keys);
				return types.ToArray();
			}
		}

		public bool isInvulnerable => m_isInvulnerable || m_externalInvulnerabilities.Count > 0;

		public bool isDead => m_value == 0f;

		public UnityEvent<HealthEventArgs> onValueChanging => m_onValueChanging;
		public UnityEvent<HealthEventArgs> onValueChanged => m_onValueChanged;
		public UnityEvent<HealthEventArgs> onHealed => m_onHealed;
		public UnityEvent<HealthEventArgs> onDamaged => m_onDamaged;
		public UnityEvent<HealthEventArgs> onDying => m_onDying;
		public UnityEvent<HealthEventArgs> onDied => m_onDied;
		public UnityEvent<HealthEventArgs> onResurrected => m_onResurrected;

		#endregion

		#region Methods

		public void Recycle()
		{
			// Intentionally done silently for pooling
			m_value = maxValue;

			m_isInvulnerable = false;
			m_externalInvulnerabilities.Clear();

			Start();
		}

		protected virtual void Awake()
		{
			for (int i = m_rates.Count - 1; i >= 0; i--)
			{
				var rate = m_rates[i];
				if (rate.damageType == null)
				{
					ModifyNullRegenerateRate(-rate.value);
				}
				else
				{
					if (!m_regenerateRates.TryGetValue(rate.damageType, out var cachedRate))
					{
						m_regenerateRates.Add(rate.damageType, new DamageHit(rate.value, rate.damageType, true));
					}
					else
					{
						cachedRate.value += rate.value;
						if (cachedRate.value == 0f)
						{
							m_regenerateRates.Remove(rate.damageType);
						}
					}
				}
			}
		}

		public void Start()
		{
			if (m_startInvulnerable)
			{
				m_externalInvulnerabilities.Add(this);
			}

			UnpauseRegeneration(true);
		}

		[ContextMenu("Kill")]
		public void Kill()
		{
			Damage(m_value);
		}

		[ContextMenu("Resurrect")]
		public void Resurrect()
		{
			Heal(maxValue);
		}

		protected void SetValue(DamageHit hit, bool fromRegenerate = false)
		{
			var value = Mathf.Clamp(this.value + hit.modifiedValue, 0f, maxValue);
			var e = new HealthEventArgs(hit)
			{
				value = value,
				normalizedValue = this.value / maxValue,
				delta = value - this.value,
			};

			// Make sure victim is assigned if coming from Heal or Damage methods
			e.hit.victim = e.hit.victim ?? this;

			onValueChanging.Invoke(e);

			// Update value, normalizedValue, and delta based on pre/postDamageFactors
			e.value = Mathf.Clamp(m_value + e.GetDelta(), 0f, maxValue);
			e.normalizedValue = e.value / maxValue;
			e.delta = e.value - m_value;

			// OnValueChanging may have changed value
			// For example, Armor may have reduced value
			value = e.value;

			// No change, skip
			if (value == m_value)
				return;

			// Attempt to lose health while invulnerable, skip
			if (!fromRegenerate && value < m_value && isInvulnerable)
				return;

			bool regenerate = false;

			// Losing health...
			if (value < m_value)
			{
				// Not going to be dead
				if (value > 0f)
				{
					if (!fromRegenerate)
					{
						// ...and can be invulnerable
						if (m_invulnerabilityTime > 0f && m_invulnerabilityThread == null)
						{
							m_invulnerabilityThread = StartCoroutine(RunInvulerability());
						}

						// ...and restart timer
						regenerate = true;
					}
				}
				// Going to be dead AND stop regeneration when dead
				else if ((m_stopCondition & RegenerateStop.Zero) != 0 && !m_regenerateRates.Any())
				{
					PauseRegeneration();
				}
			}
			// Gaining health...
			else
			{
				// Not going to be at full health
				if (value < maxValue)
				{
					regenerate = !fromRegenerate;
				}
				// Going to be at full health AND stop regeneration when maxed
				else if ((m_stopCondition & RegenerateStop.Maximum) != 0 && !m_regenerateRates.Any())
				{
					PauseRegeneration();
				}
			}

			bool wasDead = isDead;
			m_value = value;

			if (regenerate)
			{
				// Wait until after value change to unpause
				// Otherwise, leaving from maxHealth does not restart thread
				UnpauseRegeneration(hit.damageType == null);
			}

			// Update to new normalized value before invoking OnValueChanged
			e.normalizedValue = normalizedValue;
			onValueChanged.Invoke(e);

			if (e.delta > 0)
			{
				m_onHealed.Invoke(e);
			}
			else
			{
				m_onDamaged.Invoke(e);
			}

			// Change victim to this health component so layers and composites can respond appropriately
			// Only for state change events
			e.hit.victim = this;

			if (isDead)
			{
				m_onDying.Invoke(e);
				m_onDied.Invoke(e);
				Killed?.Invoke(gameObject, EventArgs.Empty);
			}
			else if (wasDead)
			{
				m_onResurrected.Invoke(e);
			}
		}

		#endregion

		#region IHealth Methods

		public void Heal(float delta, DamageType damageType = null)
		{
			Apply(new DamageHit(delta, damageType));
		}

		public void Damage(float delta, DamageType damageType = null)
		{
			Apply(new DamageHit(-delta, damageType));
		}

		public void Apply(DamageHit damageInfo)
		{
			SetValue(damageInfo, false);
		}

		#endregion

		#region Invulerability Methods

		public void SetInvulerable(bool value)
		{
			SetInvulerable(this, value);
		}

		public void SetInvulerable(Component source, bool value)
		{
			if (value)
			{
				if (!m_externalInvulnerabilities.Contains(source))
				{
					m_externalInvulnerabilities.Add(source);
				}
			}
			else if (m_externalInvulnerabilities.Contains(source))
			{
				m_externalInvulnerabilities.Remove(source);
			}
		}

		private IEnumerator RunInvulerability()
		{
			// Wait until end of frame to allow for other damage to occur this frame
			// For example, impact AND splash damage
			yield return new WaitForEndOfFrame();

			m_isInvulnerable = true;
			yield return new WaitForSeconds(m_invulnerabilityTime);
			m_isInvulnerable = false;

			// Reset pointer
			m_invulnerabilityThread = null;
		}

		#endregion

		#region Regeneration Methods

		public float GetRegenerationRate()
		{
			return GetRegenerationRate(null);
		}

		public float GetRegenerationRate(DamageType damageType)
		{
			if (damageType == null)
			{
				return m_nullRegenerateRate?.value ?? 0f;
			}
			else if (m_regenerateRates.TryGetValue(damageType, out var rate))
			{
				return rate.value;
			}
			return 0f;
		}

		public void SetRegenerationRate(float value)
		{
			SetRegenerationRate(null, value);
		}

		public void SetRegenerationRate(DamageType damageType, float value)
		{
			bool resetDelay = false;
			if (damageType == null)
			{
				resetDelay = SetNullRegenerateRate(value);
			}
			else if (m_regenerateRates.TryGetValue(damageType, out var rate))
			{
				rate.value = value;
				if (rate.value == 0f)
				{
					m_regenerateRates.Remove(damageType);
				}
			}
			else
			{
				m_regenerateRates.Add(damageType, new DamageHit(value, damageType, true));
			}

			OnRegenerationChanged.Invoke();
			UpdateRegeneration(resetDelay);
		}

		public void ModifyRegenerationRate(float value)
		{
			ModifyRegenerationRate(null, value);
		}

		public void ModifyRegenerationRate(DamageType damageType, float value)
		{
			// No change, skip
			if (value == 0f)
				return;

			bool resetDelay = false;
			if (damageType == null)
			{
				resetDelay = ModifyNullRegenerateRate(value);
			}
			else if (m_regenerateRates.TryGetValue(damageType, out var rate))
			{
				rate.value += value;
				if (rate.value == 0f)
				{
					m_regenerateRates.Remove(damageType);
				}
			}
			else
			{
				m_regenerateRates.Add(damageType, new DamageHit(value, damageType, true));
			}

			OnRegenerationChanged.Invoke();
			UpdateRegeneration(resetDelay);
		}

		private bool SetNullRegenerateRate(float value)
		{
			if (m_nullRegenerateRate == null)
			{
				m_nullRegenerateRate = new DamageHit(value, null, true);
				return true;
			}
			
			m_nullRegenerateRate.value = value;
			if (m_nullRegenerateRate.value == 0f)
			{
				m_nullRegenerateRate = null;
			}

			return false;
		}

		private bool ModifyNullRegenerateRate(float value)
		{
			return SetNullRegenerateRate((m_nullRegenerateRate?.value ?? 0f) + value);
		}

		private void UpdateRegeneration(bool resetDelay)
		{
			if (canRegenerate)
			{
				UnpauseRegeneration(resetDelay);
			}
			else
			{
				PauseRegeneration();
			}
		}

		private void UnpauseRegeneration(bool resetDelay)
		{
			float r = GetRegenerationRate();
			if (r == 0f)
				return;

			// Regenerating
			if (r > 0f)
			{
				if (resetDelay)
				{
					m_remainingDelayTime = m_regenerateDelay;
				}

				if (m_regenerateThread == null
					&& (value < maxValue || (m_stopCondition & RegenerateStop.Maximum) == 0 || m_regenerateRates.Any()))
				{
					UnpauseRegeneration();
				}
			}
			// Degenerating
			else
			{
				if (resetDelay)
				{
					m_remainingDelayTime = m_degenerateDelay;
				}

				if (m_regenerateThread == null
					&& (value > 0 || (m_stopCondition & RegenerateStop.Zero) == 0 || m_regenerateRates.Any()))
				{
					UnpauseRegeneration();
				}
			}
		}

		public void PauseRegeneration()
		{
			this.CancelCoroutine(ref m_regenerateThread);
		}

		public void UnpauseRegeneration()
		{
			this.RestartCoroutine(AsyncRegeneration(), ref m_regenerateThread);
		}

		private IEnumerator AsyncRegeneration()
		{
			while (canRegenerate)
			{
				if (m_nullRegenerateRate != null)
				{
					//if (!m_regenerateRates.Any())
					//{
					//	// Stop if condition is zero AND has no health AND degenerating
					//	if ((m_stopCondition & RegenerateStop.Zero) != 0 && isDead)
					//	{
					//		m_regenerateThread = null;
					//		yield break;
					//	}

					//	// Stop if condition is maximum AND has max health AND regenerating
					//	if ((m_stopCondition & RegenerateStop.Maximum) != 0 && value == maxValue)
					//	{
					//		m_regenerateThread = null;
					//		yield break;
					//	}
					//}

					m_remainingDelayTime = Mathf.Max(m_remainingDelayTime - Time.deltaTime, 0f);
					if (m_remainingDelayTime <= 0f)
					{
						SetValue(m_nullRegenerateRate, true);
					}
				}

				// Deal damage (or heal) using each damage type
				foreach (var rate in m_regenerateRates.Values)
				{
					SetValue(rate, true);
				}

				// Wait until next frame
				yield return null;
			}

			// Reset pointer
			m_regenerateThread = null;
		}

		#endregion

		#region Editor-Only
#if UNITY_EDITOR

        [ContextMenu("Damage")]
		private void DebugDamage()
		{
			Damage(1f);
		}

        [ContextMenu("Heal")]
        private void DebugHeal()
        {
			Heal(1f);
        }

#endif
#endregion

		#region Structures

		[System.Serializable]
		public class DamageCollection : SerializableDictionary<DamageType, float>
		{ }

		#endregion
	}
}