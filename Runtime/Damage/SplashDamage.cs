using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToolkitEngine.Health
{
	[System.Serializable]
	public class SplashDamage : Damage
	{
		#region Fields

		[SerializeField]
		protected UnityFloat m_factor = new UnityFloat(1f);

		[SerializeField]
		protected float m_impulse;

		[SerializeField]
		protected float m_upwardModifier;

		[SerializeField, MinMax(0f, float.PositiveInfinity, "Inner", "Outer")]
		protected Vector2 m_radius;

		[SerializeField]
		protected AnimationCurve m_falloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

		[SerializeField]
		protected List<Damage> m_bonuses;

		#endregion

		#region Properties

		public float innerRadius => m_radius.x;
		public float outerRadius => m_radius.y;

		#endregion

		#region Constructors

		public SplashDamage()
		{ }

		public SplashDamage(SplashDamage other)
		{
			m_value = other.value;
			m_damageType = other.m_damageType;
			m_factor = new UnityFloat(other.m_factor.value);
			m_upwardModifier = other.m_upwardModifier;
			m_radius = other.m_radius;
			m_falloff = other.m_falloff;
			m_bonuses = new List<Damage>(other.m_bonuses);
		}

		#endregion

		#region Methods

		public DamageHit[] Apply(Vector3 point, GameObject source)
		{
			List<DamageHit> hits = new();
			if (outerRadius > 0f)
			{
				// Cache factor because reflection may be involved
				float factor = m_factor.value;

				HashSet<IDamageReceiver> victims = new();
				HashSet<Rigidbody> rigidbodies = new();

				foreach (var collider in Physics.OverlapSphere(point, outerRadius)
					.OrderBy(x => (x.transform.position - point).sqrMagnitude))
				{
					// TODO: Block damage if damaged through all; will need blocking layers
					//// Collider in range, but blocked by object
					//if (!Physics.RaycastAll(point, direction, out RaycastHit hit, m_radius, ~0, QueryTriggerInteraction.Ignore))
					//	continue;

					var victim = collider.GetComponentInParent<IDamageReceiver>();
					if (victim != null)
					{
						// Health has already been damaged, skip
						if (victims.Contains(victim))
							continue;

						// Remember who was hit
						victims.Add(victim);

						var hit = new DamageHit(this)
						{
							damageType = damageType,
							origin = point,
							contact = collider.transform.position,
							source = source,
							victim = victim,
							collider = collider,
						};
						hits.Add(hit);

						float falloffFactor = 1f;
						if (value != 0f && factor != 0f)
						{
							var sqrDistance = (point - collider.transform.position).sqrMagnitude;
							falloffFactor = m_falloff.Evaluate(MathUtil.GetPercent(sqrDistance, innerRadius * innerRadius, outerRadius * outerRadius));
						}

						hit.value = -value * factor * falloffFactor;
						victim.Apply(hit);

						// Apply bonus splash damage
						foreach (var bonus in m_bonuses)
						{
							victim.Apply(new DamageHit(-bonus.value * factor, damageType, hit));
						}
					}

					if (m_impulse != 0f)
					{
						var rigidbody = collider.GetComponentInParent<Rigidbody>();
						if (rigidbody != null && !rigidbodies.Contains(rigidbody))
						{
							rigidbody.AddExplosionForce(m_impulse, point, outerRadius, m_upwardModifier, ForceMode.Impulse);
							rigidbodies.Add(rigidbody);
						}
					}
				}
			}

			return hits.ToArray();
		}

		#endregion
	}
}