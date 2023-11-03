using System;
using System.Collections;
using System.Collections.Generic;
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

		#endregion

		#region Constructors

		public HealthEventArgs()
		{ }

		public HealthEventArgs(DamageHit hit)
		{
			this.hit = hit;
		}

		public HealthEventArgs(HealthEventArgs copy)
		{
			value = copy.value;
			normalizedValue = copy.normalizedValue;
			delta = copy.delta;
			preDamageFactor = copy.preDamageFactor;
			postDamageFactor = copy.postDamageFactor;
			hit = new DamageHit(copy.hit);
		}

		#endregion

		#region Methods

		public float GetDelta()
		{
			return IHealthModifier.GetDelta(
				// Pre-damage factor cannot be less than 0 (cannot heal from faction and/or difficulty scaling)
				// But negative value still needs to be considered earlier for proper modifications
				IHealthModifier.GetDelta(hit.value, Mathf.Max(0f, preDamageFactor)),
				postDamageFactor);
		}

		#endregion
	}

	public class Health : MonoBehaviour, IHealth, IHealthRegeneration, IPoolItemRecyclable
    {
		#region Fields

		[SerializeField, Min(0f)]
		private float m_value = 100f;

		[SerializeField, Min(0f)]
		private float m_maxValue = 100f;

		[SerializeField, Min(0f), Tooltip("Seconds of invulnerability after receiving damage.")]
		private float m_invulnerabilityTime;

		[SerializeField, Tooltip("Indicates whether health cannot initially receive damage.")]
		private bool m_startInvulnerable;

        [SerializeField, Tooltip("Indicates whether health can regenerate (or degenerate).")]
        private bool m_canRegenerate;

        /// <summary>
        /// Seconds before regeneration begins
        /// </summary>
        [SerializeField, Min(0f), Tooltip("Seconds before regeneration begins (only used for positive regeneration rate).")]
		private float m_regenerateDelay;

		[SerializeField, Min(0f), Tooltip("Seconds before degeneration begins (only used for negative regeneration rate).")]
		private float m_degenerateDelay;

		[SerializeField, Tooltip("Health regenerated (or degenerated) per second by damage type.")]
		private Damage[] m_rates;

		[SerializeField, Tooltip("Indicates whether regeneration can occur while dead.")]
		private bool m_canRegenerateDead = false;

        private bool m_isInvulnerable = false;
        private HashSet<Component> m_externalInvulnerabilities = new();
        private Coroutine m_invulnerabilityThread = null;

        private Coroutine m_regenerationThread = null;
		private float m_regenerationRate;

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

		public float maxValue
        {
			get => m_maxValue;
			set
            {
				// No change, skip
				if (m_maxValue == value)
					return;

				m_maxValue = value;

				float prev = m_value;
				m_value = Mathf.Min(m_value, m_maxValue);

				var eventArgs = new HealthEventArgs()
				{
					value = m_value,
					normalizedValue = m_value / m_maxValue,
					delta = prev - m_value,
				};
				onValueChanged.Invoke(eventArgs);

				if (isDead)
                {
					this.CancelCoroutine(ref m_invulnerabilityThread);
					if (!m_canRegenerateDead)
					{
                        this.CancelCoroutine(ref m_regenerationThread);
                    }
					onDied.Invoke(eventArgs);
                }
            }
		}

		public float normalizedValue => m_value / m_maxValue;

		public bool canRegenerate
		{
			get => m_canRegenerate;
			set
			{
				// No change, skip
				if (m_canRegenerate == value)
					return;

				m_canRegenerate = value;

				if (value && m_regenerationRate != 0f)
				{
					m_regenerationThread = StartCoroutine(AsyncRegeneration());
				}
				else
				{
					this.CancelCoroutine(ref m_regenerationThread);
				}
			}
		}

        public float regenerationRate
        {
			get => m_regenerationRate;
			set
            {
				// No change, skip
				if (m_regenerationRate == value)
					return;

				var previousRate = m_regenerationRate;

				m_regenerationRate = value;
				OnRegenerationChanged.Invoke();

				if (m_canRegenerate)
				{
					// No change, stop coroutine
					if (m_regenerationRate == 0f)
					{
						this.CancelCoroutine(ref m_regenerationThread);
					}
					// Regeneration sign changed
					// Need to restart coroutine to wait for delay (or cancel delay)
					else if (previousRate == 0f || (previousRate > 0 ^ m_regenerationRate > 0f))
					{
						this.RestartCoroutine(AsyncRegeneration(), ref m_regenerationThread);
					}
				}
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
			m_value = m_maxValue;

			m_isInvulnerable = false;
            m_externalInvulnerabilities.Clear();

			Start();
        }

		public void Start()
        {
			if (m_startInvulnerable)
			{
				m_externalInvulnerabilities.Add(this);
			}

			if (m_canRegenerate && m_regenerationRate != 0f)
			{
				this.RestartCoroutine(AsyncRegeneration(), ref m_regenerationThread);
			}
		}

        [ContextMenu("Kill")]
		public void Kill()
		{
			Damage(m_value);
		}

        [ContextMenu("Resurrect")]
        public void Resurrect()
		{
			Heal(m_maxValue);
		}

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

		protected void SetValue(DamageHit hit, bool ignoreInvulnerability = false)
		{
			var value = Mathf.Clamp(this.value + hit.value, 0f, m_maxValue);
			var e = new HealthEventArgs(hit)
			{
				value = value,
				normalizedValue = this.value / m_maxValue,
				delta = value - this.value,
			};

			// Make sure victim is assigned if coming from Heal or Damage methods
			e.hit.victim = e.hit.victim ?? this;

			onValueChanging.Invoke(e);

			// Update value, normalizedValue, and delta based on pre/postDamageFactors
			e.value = Mathf.Clamp(m_value + e.GetDelta(), 0f, m_maxValue);
			e.normalizedValue = e.value / m_maxValue;
			e.delta = e.value - m_value;

            // OnValueChanging may have changed value
            // For example, Armor may have reduced value
            value = e.value;

			// No change, skip
			if (value == m_value)
				return;

			// Attempt to lose health while invulnerable, skip
			if (!ignoreInvulnerability && value < m_value && isInvulnerable)
				return;

			// Losing health...
			if (value < m_value)
			{
				// Not going to be dead
				if (value > 0f || m_canRegenerateDead)
                {
					/// ...and can be invulnerable...
					if (m_invulnerabilityTime > 0f && m_invulnerabilityThread == null)
					{
						m_invulnerabilityThread = StartCoroutine(RunInvulerability());
					}

					// ...but has regeneration...
					if (m_regenerationRate > 0f)
					{
						RestartRegeneration();
					}
                }
			}
			// Gaining health but has degeneration...
			else if (m_regenerationRate < 0f)
            {
				RestartRegeneration();
            }

			bool wasDead = isDead;
			m_value = value;

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

		public void RestartRegeneration()
		{
            if (m_canRegenerate)
            {
                this.RestartCoroutine(AsyncRegeneration(), ref m_regenerationThread);
            }
        }

		public void CancelRegeneration()
		{
			if (m_canRegenerate)
			{
                this.CancelCoroutine(ref m_regenerationThread);
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

		private IEnumerator AsyncRegeneration()
        {
			float delay = 0f;
			Func<bool> condition = null;
			if (m_regenerationRate > 0f)
            {
				delay = m_regenerateDelay;
				condition = () => m_value < m_maxValue;
            }
			else if (m_regenerationRate < 0f)
            {
				delay = m_degenerateDelay;
				condition = () => m_value > 0f;
            }

			if (delay > 0f)
            {
				yield return new WaitForSeconds(delay);
			}

			//while ((!IsDead || (m_canRegenerateDead && m_regenerationRate > 0f)) && (condition?.Invoke() ?? false))
			//{
			//	SetValue(m_value + m_regenerationRate * Time.deltaTime, null, null, Vector3.zero, Vector3.zero, null, true);
			//	yield return null;
			//}

			// Reset pointer
			m_regenerationThread = null;
        }

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