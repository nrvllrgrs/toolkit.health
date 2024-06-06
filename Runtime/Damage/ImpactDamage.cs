using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Health
{
	[System.Serializable]
	public class ImpactDamage : Damage, IBonusDamageContainer
	{
		#region Fields

		[SerializeField]
		protected UnityFloat m_factor = new UnityFloat(1f);

		[SerializeField]
		protected float m_impulse;

		[SerializeField, MaxInfinity, Min(-1f)]
		protected float m_range = float.PositiveInfinity;

		[SerializeField]
		protected AnimationCurve m_falloff = AnimationCurve.Constant(0f, 1f, 1f);

		[SerializeField]
		protected List<Damage> m_bonuses;

		#endregion

		#region Properties

		public float factor
		{
			get => m_factor.value;
			set => m_factor.value = value;
		}

		public float impulse { get => m_impulse; set => m_impulse = value; }

		public float range
		{
			get
			{
				return m_range > 0f
					? m_range
					: float.PositiveInfinity;
			}
			set
			{
				m_range = value > 0f
					? value
					: float.PositiveInfinity;
			}
		}

		public float visualRange
		{
			get
			{
				return !hasInfiniteRange
					? m_range
					: Camera.main.farClipPlane;
			}
		}

		public bool hasInfiniteRange => range == float.PositiveInfinity;

		public List<Damage> bonuses => m_bonuses;

		#endregion

		#region Constructors

		public ImpactDamage()
		{ }

		public ImpactDamage(float value, DamageType damageType, float factor, float range, AnimationCurve falloff)
			: base(value, damageType)
		{
			m_factor = new UnityFloat(factor);
			m_range = range;
			m_falloff = falloff;
		}

		public ImpactDamage(ImpactDamage other)
		{
			other.CopyTo(this);
		}

		#endregion

		#region Methods

		public override void CopyTo(Damage destination)
		{
			if (destination == null)
				return;

			if (destination is ImpactDamage dstImpactDamage)
			{
				dstImpactDamage.m_value = m_value;
				dstImpactDamage.m_damageType = m_damageType;
				dstImpactDamage.m_factor = new UnityFloat(m_factor.value);
				dstImpactDamage.m_impulse = m_impulse;
				dstImpactDamage.m_range = m_range;
				dstImpactDamage.m_falloff = m_falloff;
				dstImpactDamage.m_bonuses = new List<Damage>(m_bonuses);
			}
		}

		public bool Apply(DamageHit hit, IDamageDealer dealer = null)
		{
			if (hit == null)
				return false;

			bool anyApplied = false;
			if (hit.victim != null)
			{
				float factor = m_factor.value;
				if (m_value != 0f && factor != 0f)
				{
					if (m_range > 0f && m_falloff != null)
					{
						factor *= m_falloff.Evaluate(hit.distance / m_range);
					}
				}

				hit.value *= factor;

				InvokeDamageDealing(hit, dealer);
				hit.victim.Apply(hit);
				InvokeDamageDealt(hit, dealer, ref anyApplied);

				// Apply bonus impact damages
				foreach (var bonus in m_bonuses)
				{
					var bonusHit = new DamageHit(bonus.value * factor, damageType, hit);
					hit.victim.Apply(bonusHit);

					InvokeDamageDealt(bonusHit, dealer, ref anyApplied);
				}
			}

			if (m_impulse != 0f)
			{
				var rigidbody = hit.collider.GetComponentInParent<Rigidbody>();
				if (rigidbody != null)
				{
					var force = -hit.normal * m_impulse;
					rigidbody.AddForceAtPosition(force, hit.contact, ForceMode.Impulse);
				}
			}

			return anyApplied;
		}

		private void InvokeDamageDealing(DamageHit hit, IDamageDealer dealer)
		{
			if (hit.value != 0f)
			{
				dealer?.onDamageDealing?.Invoke(new HealthEventArgs(hit, dealer?.transform.gameObject));
			}
		}

		private void InvokeDamageDealt(DamageHit hit, IDamageDealer dealer, ref bool anyApplied)
		{
			if (hit.value != 0f)
			{
				dealer?.onDamageDealt?.Invoke(new HealthEventArgs(hit, dealer?.transform.gameObject));
				anyApplied = true;
			}
		}

		#endregion
	}
}