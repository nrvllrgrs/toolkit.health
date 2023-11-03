using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Health
{
	[System.Serializable]
	public class ImpactDamage : Damage
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

		public float impulse { get => m_impulse; set => m_impulse = value; }

		public float factor => m_factor.value;

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


		#endregion

		#region Constructors

		public ImpactDamage()
		{ }

		public ImpactDamage(ImpactDamage other)
		{
			m_value = other.m_value;
			m_damageType = other.m_damageType;
			m_factor = new UnityFloat(other.m_factor.value);
			m_impulse = other.m_impulse;
			m_range = other.m_range;
			m_falloff = other.m_falloff;
			m_bonuses = new List<Damage>(other.m_bonuses);
		}

		#endregion

		#region Methods

		public void Apply(DamageHit hit)
		{
			if (hit == null)
				return;

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
				hit.victim.Apply(hit);

				// Apply bonus impact damages
				foreach (var bonus in m_bonuses)
				{
					hit.victim.Apply(new DamageHit(-bonus.value * factor, damageType, hit));
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
		}

		#endregion
	}
}