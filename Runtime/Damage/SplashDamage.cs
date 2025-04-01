using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToolkitEngine.Health
{
	[System.Serializable]
	public class SplashDamage : Damage, IBonusDamageContainer
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

		public float factor
		{
			get => m_factor.value;
			set => m_factor.value = value;
		}

		public float impulse { get => m_impulse; set => m_impulse = value; }
		public float upwardModifier { get => m_upwardModifier; set => m_upwardModifier = value; }

		public float innerRadius { get => m_radius.x; set => m_radius.x = value; }
		public float outerRadius { get => m_radius.y; set => m_radius.y = value; }

		public List<Damage> bonuses => m_bonuses;

		#endregion

		#region Constructors

		public SplashDamage()
		{ }

		public SplashDamage(float value, DamageType damageType, float factor, float impulse, float upwardModifier, float innerRadius, float outerRadius, AnimationCurve falloff)
			: base(value, damageType)
		{
			m_factor = new UnityFloat(factor);
			m_impulse = impulse;
			m_upwardModifier = upwardModifier;
			m_radius = new Vector2(innerRadius, outerRadius);
			m_falloff = falloff;
			m_bonuses = new();
		}

		public SplashDamage(SplashDamage other)
		{
			other.CopyTo(this);
		}

		#endregion

		#region Methods

		public override void CopyTo(Damage destination)
		{
			if (destination == null)
				return;

			if (destination is SplashDamage dstSplashDamage)
			{
				dstSplashDamage.m_value = value;
				dstSplashDamage.m_damageType = m_damageType;
				dstSplashDamage.m_factor = new UnityFloat(m_factor.value);
				dstSplashDamage.m_upwardModifier = m_upwardModifier;
				dstSplashDamage.m_radius = m_radius;
				dstSplashDamage.m_falloff = m_falloff;
				dstSplashDamage.m_bonuses = new List<Damage>(m_bonuses);
			}
		}

		public bool Apply(Vector3 point, GameObject source, out DamageHit[] hits, int layerMask = ~0, IDamageDealer dealer = null)
		{
			bool anyApplied = false;

			List<DamageHit> list = new();
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

					// Collider is not on valid layer, skip
					if ((layerMask & 1 << collider.gameObject.layer) == 0)
						continue;

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
						list.Add(hit);

						float falloffFactor = 1f;
						if (value != 0f && factor != 0f)
						{
							var sqrDistance = (point - collider.transform.position).sqrMagnitude;
							falloffFactor = m_falloff.Evaluate(MathUtil.GetPercent(sqrDistance, innerRadius * innerRadius, outerRadius * outerRadius));
						}

						hit.value = -value * factor * falloffFactor;

						InvokeDamageDealing(hit, dealer);
						Debug.Log($"Damaging {victim.transform.name}; Value = {hit.value}");
						victim.Apply(hit);
						InvokeDamageDealt(hit, dealer, ref anyApplied);

						// Apply bonus splash damage
						foreach (var bonus in m_bonuses)
						{
							var bonusHit = new DamageHit(-bonus.value * factor, damageType, hit);
							victim.Apply(bonusHit);

							InvokeDamageDealt(bonusHit, dealer, ref anyApplied);
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

			hits = list.ToArray();
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