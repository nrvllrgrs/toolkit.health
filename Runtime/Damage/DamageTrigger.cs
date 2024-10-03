using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace ToolkitEngine.Health
{
	public class DamageTrigger : MonoBehaviour
    {
		#region Fields

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

		public UnityEvent<HealthEventArgs> onDamageDealing => m_onDamageDealing;
		public UnityEvent<HealthEventArgs> onDamageDealt => m_onDamageDealt;

		#endregion

		#region Methods

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (m_enterDamage.value == 0f)
				return;

			ApplyDamage(m_enterDamage, false, other.GetComponentInParent<IHealth>(), other);
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (m_stayDamage.value == 0f)
				return;

			ApplyDamage(m_stayDamage, true, other.GetComponentInParent<IHealth>(), other);
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
	}
}