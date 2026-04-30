using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health.Netcode
{
	[AddComponentMenu("Health/Netcode/Network Health")]
	public class NetworkHealth : NetworkBehaviour, IHealth
	{
		#region Fields

		[SerializeField]
		private Health m_health;

		/// <summary>
		/// Networked health value
		/// </summary>
		readonly NetworkVariable<float> m_healthValue = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

		/// <summary>
		/// Networked bonus value
		/// </summary>
		readonly NetworkVariable<float> m_bonusValue = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

		/// <summary>
		/// Networked maximum value
		/// </summary>
		readonly NetworkVariable<float> m_maxValue = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

		#endregion

		#region Events

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

		public event EventHandler Killed;

		#endregion

		#region Properties

		public Health health
		{
			get => m_health;
			set
			{
				// No change, skip
				if (m_health == value)
					return;

				Unregister();
				m_health = value;
				Register();
			}
		}

		public float value
		{
			get => m_healthValue.Value;
			set
			{
				// Only the owner may write NetworkVariables
				if (!IsOwner)
					return;

				m_healthValue.Value = value;
			}
		}

		public float bonusValue => m_bonusValue.Value;
		public float rawMaxValue => m_maxValue.Value;
		public float maxValue => rawMaxValue + bonusValue;
		public float normalizedValue => maxValue != 0f ? value / maxValue : 0f;

		// NetworkHealth does not support pre-change interception; expose a no-op event
		// rather than null so callers can safely call AddListener without null-checking.
		public UnityEvent<HealthEventArgs> onValueChanging { get; } = new UnityEvent<HealthEventArgs>();
		public UnityEvent<HealthEventArgs> onValueChanged => m_onValueChanged;
		public UnityEvent<HealthEventArgs> onDamaged => m_onDamaged;
		public UnityEvent<HealthEventArgs> onHealed => m_onHealed;
		public UnityEvent<HealthEventArgs> onDying => m_onDying;
		public UnityEvent<HealthEventArgs> onDied => m_onDied;
		public UnityEvent<HealthEventArgs> onResurrected => m_onResurrected;

		public bool isDead => value == 0f;

		#endregion

		#region Methods

		private void Awake()
		{
			if (m_health == null)
			{
				m_health = GetComponent<Health>();
			}
		}

		private void Register()
		{
			if (IsOwner)
			{
				if (m_health != null)
				{
					m_maxValue.Value = health.rawMaxValue;
					m_healthValue.Value = health.value;
					m_bonusValue.Value = health.bonusValue;
					//Debug.Log($"Register health: OwnerClientId = {OwnerClientId}; Value = {value}");

					health.onValueChanged.AddListener(Owner_Health_ValueChanged);
					health.onDamaged.AddListener(Owner_Health_Damaged);
					health.onHealed.AddListener(Owner_Health_Healed);
					health.onDying.AddListener(Owner_Health_Dying);
					health.onDied.AddListener(Owner_Health_Died);
					health.onResurrected.AddListener(Owner_Health_Resurrected);
				}
			}
			else
			{
				//Debug.Log($"Register health: Non-OwnerClientId = {OwnerClientId}; Value = {value}; Bonus = {bonusValue}; Max = {rawMaxValue}");
				m_healthValue.OnValueChanged += Health_ValueChanged;

				// Bonus and max changes only affect normalized value; use a separate
				// handler so their raw deltas are not misread as heal/damage amounts.
				m_bonusValue.OnValueChanged += Health_AuxValueChanged;
				m_maxValue.OnValueChanged += Health_AuxValueChanged;

				Health_ValueChanged(0f, value);
			}
		}

		private void Unregister(bool wasOwner = false)
		{
			if (IsOwner || wasOwner)
			{
				if (m_health != null)
				{
					//Debug.Log($"Unregister health: OwnerClientId = {OwnerClientId}");
					health.onValueChanged.RemoveListener(Owner_Health_ValueChanged);
					health.onDamaged.RemoveListener(Owner_Health_Damaged);
					health.onHealed.RemoveListener(Owner_Health_Healed);
					health.onDying.RemoveListener(Owner_Health_Dying);
					health.onDied.RemoveListener(Owner_Health_Died);
					health.onResurrected.RemoveListener(Owner_Health_Resurrected);
				}
			}
			else
			{
				//Debug.Log($"Unregister health: Non-OwnerClientId = {OwnerClientId}");
				m_healthValue.OnValueChanged -= Health_ValueChanged;
				m_bonusValue.OnValueChanged -= Health_AuxValueChanged;
				m_maxValue.OnValueChanged -= Health_AuxValueChanged;
			}
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			Register();
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			Unregister();
		}

		public override void OnGainedOwnership()
		{
			base.OnGainedOwnership();

			// Must remove non-owner NetworkVariable listeners before registering
			// as owner, otherwise both paths fire simultaneously.
			Unregister();
			Register();
		}

		public override void OnLostOwnership()
		{
			base.OnLostOwnership();

			// wasOwner: true ensures the owner Health listeners are removed even
			// though IsOwner is already false by the time this callback fires.
			Unregister(true);

			// Re-register as a non-owner so NetworkVariable callbacks are active.
			Register();
		}

		#endregion

		#region IHealth Methods

		public void Apply(DamageHit damageInfo)
		{
			// Intentionally left blank
		}

		public void Damage(float delta, DamageType damageType = null)
		{
			// Intentionally left blank
		}

		public void Heal(float delta, DamageType damageType = null)
		{
			// Intentionally left blank
		}

		#endregion

		#region Owner Callbacks

		private void Owner_Health_ValueChanged(HealthEventArgs e)
		{
			m_healthValue.Value = e.value;
			m_onValueChanged?.Invoke(e);

			if (m_bonusValue.Value != m_health.bonusValue)
			{
				m_bonusValue.Value = m_health.bonusValue;
			}

			if (m_maxValue.Value != m_health.rawMaxValue)
			{
				m_maxValue.Value = m_health.rawMaxValue;
			}
		}

		private void Owner_Health_Damaged(HealthEventArgs e) => m_onDamaged?.Invoke(e);
		private void Owner_Health_Healed(HealthEventArgs e) => m_onHealed?.Invoke(e);
		private void Owner_Health_Dying(HealthEventArgs e) => m_onDying?.Invoke(e);
		private void Owner_Health_Died(HealthEventArgs e) => m_onDied?.Invoke(e);
		private void Owner_Health_Resurrected(HealthEventArgs e) => m_onResurrected?.Invoke(e);

		#endregion

		#region Network Callbacks

		/// <summary>
		/// Called on non-owner clients when the health value changes.
		/// Drives all heal/damage/death/resurrection events.
		/// </summary>
		private void Health_ValueChanged(float previous, float value)
		{
			float delta = value - previous;
			var e = new HealthEventArgs()
			{
				value = this.value,
				normalizedValue = normalizedValue,
				delta = delta,
			};

			m_onValueChanged?.Invoke(e);
			//Debug.Log($"Value Changed: Value = {this.value}; Bonus = {bonusValue}; Max = {rawMaxValue}");

			if (delta > 0)
			{
				m_onHealed?.Invoke(e);
			}
			else
			{
				m_onDamaged?.Invoke(e);
			}

			if (isDead)
			{
				m_onDying?.Invoke(e);
				m_onDied?.Invoke(e);
				Killed?.Invoke(gameObject, EventArgs.Empty);
			}
			else if (previous == 0f)
			{
				m_onResurrected?.Invoke(e);
			}
		}

		/// <summary>
		/// Called on non-owner clients when bonus or max value changes.
		/// Only refreshes the value-changed notification — these variables carry
		/// their own deltas, not health deltas, so they must not drive
		/// heal/damage/death events.
		/// </summary>
		private void Health_AuxValueChanged(float previous, float value)
		{
			var e = new HealthEventArgs()
			{
				value = this.value,
				normalizedValue = normalizedValue,
				delta = 0f,
			};

			m_onValueChanged?.Invoke(e);
		}

		#endregion
	}
}