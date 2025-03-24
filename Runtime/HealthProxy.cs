using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
	[AddComponentMenu("Health/Health Proxy")]
	public class HealthProxy : MonoBehaviour, IHealth, IHealthRegeneration
    {
		#region Fields

		[SerializeField]
		private Health m_health;

		#endregion

		#region Events

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onValueChanging;

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onValueChanged;

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onDamaged;

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onHealed;

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onDying;

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onDied;

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onResurrected;

		[SerializeField, Foldout("Events")]
		private UnityEvent m_onRegenerationChanged;

		#endregion

		#region Properties

		public float value
		{
			get => m_health?.value ?? -1f;
			set
			{
				if (m_health != null)
				{
					m_health.value = value;
				}
			}
		}

		public float maxValue => m_health?.maxValue ?? -1f;

		public float normalizedValue => m_health?.normalizedValue ?? -1f;

		public bool isDead => m_health?.isDead ?? false;

		public UnityEvent<HealthEventArgs> onValueChanging => m_onValueChanging;

		public UnityEvent<HealthEventArgs> onValueChanged => m_onValueChanged;

		public UnityEvent<HealthEventArgs> onDamaged => m_onDamaged;

		public UnityEvent<HealthEventArgs> onHealed => m_onHealed;

		public UnityEvent<HealthEventArgs> onDying => m_onDying;

		public UnityEvent<HealthEventArgs> onDied => m_onDied;

		public UnityEvent<HealthEventArgs> onResurrected => m_onResurrected;

		public bool canRegenerate => m_health?.canRegenerate ?? false;

		public float regenerateDelay
		{
			get => m_health?.regenerateDelay ?? 0f;
			set
			{
				if (m_health != null)
				{
					m_health.regenerateDelay = value;
				}
			}
		}

		public float degenerateDelay
		{
			get => m_health?.degenerateDelay ?? 0f;
			set
			{
				if (m_health != null)
				{
					m_health.degenerateDelay = value;
				}
			}
		}

		public DamageType[] regenerateDamageTypes => m_health?.regenerateDamageTypes;
		public UnityEvent onRegenerationChanged => m_onRegenerationChanged;

		public event EventHandler Killed;

		#endregion

		#region Methods

		private void Awake()
		{
			Register();
		}

		public void SetHealth(Health health)
		{
			Unregister();
			m_health = health;
			Register();
		}

		#endregion

		#region IHealth Methods

		public void Apply(DamageHit damageInfo)
		{
			m_health.Apply(damageInfo);
		}

		public void Damage(float delta, DamageType damageType = null)
		{
			m_health?.Damage(delta, damageType);
		}

		public void Heal(float delta, DamageType damageType = null)
		{
			m_health?.Heal(delta, damageType);
		}

		#endregion

		#region IHealthRegeneration Methods

		public float GetRegenerationRate()
		{
			return m_health?.GetRegenerationRate() ?? 0f;
		}

		public float GetRegenerationRate(DamageType damageType)
		{
			return m_health?.GetRegenerationRate(damageType) ?? 0f;
		}

		public void SetRegenerationRate(float value)
		{
			m_health?.SetRegenerationRate(value);
		}

		public void SetRegenerationRate(DamageType damageType, float value)
		{
			m_health?.SetRegenerationRate(damageType, value);
		}

		public void ModifyRegenerationRate(float value)
		{
			m_health?.ModifyRegenerationRate(value);
		}

		public void ModifyRegenerationRate(DamageType damageType, float value)
		{
			m_health.ModifyRegenerationRate(damageType, value);
		}

		public void PauseRegeneration()
		{
			m_health?.PauseRegeneration();
		}

		public void UnpauseRegeneration()
		{
			m_health?.UnpauseRegeneration();
		}

		#endregion

		#region Callbacks

		private void Register()
		{
			if (m_health == null)
				return;

			m_health.onValueChanging.AddListener(Health_ValueChanging);
			m_health.onValueChanged.AddListener(Health_ValueChanged);
			m_health.onDamaged.AddListener(Health_Damaged);
			m_health.onHealed.AddListener(Health_Healed);
			m_health.onDying.AddListener(Health_Dying);
			m_health.onDied.AddListener(Health_Died);
			m_health.onResurrected.AddListener(Health_Resurrected);
			m_health.onRegenerationChanged.AddListener(Health_RegenerationChanged);
			m_health.Killed += Health_Killed;
		}

		private void Unregister()
		{
			if (m_health == null)
				return;

			m_health.onValueChanging.RemoveListener(Health_ValueChanging);
			m_health.onValueChanged.RemoveListener(Health_ValueChanged);
			m_health.onDamaged.RemoveListener(Health_Damaged);
			m_health.onHealed.RemoveListener(Health_Healed);
			m_health.onDying.RemoveListener(Health_Dying);
			m_health.onDied.RemoveListener(Health_Died);
			m_health.onResurrected.RemoveListener(Health_Resurrected);
			m_health.onRegenerationChanged.RemoveListener(Health_RegenerationChanged);
			m_health.Killed -= Health_Killed;
		}

		private void Health_ValueChanging(HealthEventArgs e)
		{
			m_onValueChanging?.Invoke(e);
		}

		private void Health_ValueChanged(HealthEventArgs e)
		{
			m_onValueChanged?.Invoke(e);
		}

		private void Health_Damaged(HealthEventArgs e)
		{
			m_onDamaged?.Invoke(e);
		}

		private void Health_Healed(HealthEventArgs e)
		{
			m_onHealed?.Invoke(e);
		}

		private void Health_Dying(HealthEventArgs e)
		{
			m_onDying?.Invoke(e);
		}

		private void Health_Died(HealthEventArgs e)
		{
			m_onDied?.Invoke(e);
		}

		private void Health_Resurrected(HealthEventArgs e)
		{
			m_onResurrected?.Invoke(e);
		}

		private void Health_RegenerationChanged()
		{
			m_onRegenerationChanged?.Invoke();
		}

		private void Health_Killed(object sender, EventArgs e)
		{
			Killed?.Invoke(sender, e);
		}

		#endregion
	}
}