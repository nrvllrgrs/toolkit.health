using UnityEngine;
using UnityEngine.Events;
using ToolkitEngine.Sensors;
using NaughtyAttributes;

namespace ToolkitEngine.Health
{
	public class DamageTrigger : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		private BaseSensor m_sensor;

		[SerializeField]
		protected Damage m_enterDamage;

		[SerializeField]
		protected Damage m_stayDamage;

		#endregion

		#region Events

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onDamageDealing;

		[SerializeField, Foldout("Events")]
		private UnityEvent<HealthEventArgs> m_onDamageDealt;

		#endregion

		#region Properties

		private bool useSensor => m_sensor != null;
		public UnityEvent<HealthEventArgs> onDamageDealing => m_onDamageDealing;
		public UnityEvent<HealthEventArgs> onDamageDealt => m_onDamageDealt;

		#endregion

		#region Methods

		protected virtual void OnEnable()
		{
			if (useSensor)
			{
				m_sensor.onSignalDetected.AddListener(Sensor_SignalDetected);

				if (m_sensor is IPulseableSensor pulseableSensor)
				{
					pulseableSensor.onPulsed.AddListener(Sensor_Pulsed);
				}
			}
		}

		protected virtual void OnDisable()
		{
			if (useSensor)
			{
				m_sensor.onSignalDetected.RemoveListener(Sensor_SignalDetected);

				if (m_sensor is IPulseableSensor pulseableSensor)
				{
					pulseableSensor.onPulsed.RemoveListener(Sensor_Pulsed);
				}
			}
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			AttemptApplyDamage(m_enterDamage, false, other);
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			AttemptApplyDamage(m_stayDamage, true, other);
		}

		private void AttemptApplyDamage(Damage damage, bool continuous, Collider other)
		{
			if (useSensor || damage.value == 0f)
				return;

			ApplyDamage(damage, continuous, other.GetComponentInParent<IHealth>(), other);
		}

		private void ApplyDamage(Damage damage, bool continuous, IHealth victim, Collider collider)
		{
			if (victim == null)
				return;

			DamageHit damageHit = new DamageHit(damage, continuous);
			damageHit.victim = victim;
			damageHit.collider = collider;

			HealthEventArgs args = new HealthEventArgs(damageHit, gameObject);
			m_onDamageDealing?.Invoke(args);

			victim.Apply(damageHit);

			m_onDamageDealt?.Invoke(args);
		}

		#endregion

		#region Sensor Callbacks

		private void Sensor_SignalDetected(SensorEventArgs e)
		{
			AttemptApplyDamage(e, m_enterDamage, false);
		}

		private void Sensor_Pulsed(SensorEventArgs e)
		{
			AttemptApplyDamage(e, m_stayDamage, true);
		}

		private void AttemptApplyDamage(SensorEventArgs e, Damage damage, bool continuous)
		{
			if (damage.value == 0f)
				return;

			ApplyDamage(damage, continuous, e.signal.detected.GetComponentInParent<IHealth>(), e.signal.detected.GetComponent<Collider>());
		}

		#endregion
	}
}