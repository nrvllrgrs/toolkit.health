using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
	public class Explosion : MonoBehaviour, IDamageDealer, IExplosive
	{
		#region Fields

		[SerializeField]
		private LayerMask m_layerMask = ~0;

		[SerializeField]
		private SplashDamage m_damage;

		[SerializeField]
		private GameObject m_source;

		[SerializeField]
		private Spawner m_spawner;

		#endregion

		#region Events

		[SerializeField]
		private UnityEvent<Explosion> m_onDetonated;

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDamageDealing;

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDamageDealt;

		#endregion

		#region Properties

		public SplashDamage damage => m_damage;
		public UnityEvent<Explosion> onDetonated => m_onDetonated;
		public UnityEvent<HealthEventArgs> onDamageDealing => m_onDamageDealing;
		public UnityEvent<HealthEventArgs> onDamageDealt => m_onDamageDealt;

		#endregion

		#region Methods

		[ContextMenu("Detonate")]
		public void Detonate()
		{
			m_damage.Apply(transform.position, !m_source.IsNull() ? m_source : null, out var hits, m_layerMask, this);
			if (m_spawner.isDefined)
			{
				m_spawner.Instantiate(transform.position, Quaternion.identity);
			}

			m_onDetonated?.Invoke(this);
		}

		public static void Detonate(SplashDamage damage, Vector3 position, out DamageHit[] hits, int layerMask = ~0)
		{
			Detonate(damage, position, null, out hits, layerMask);
		}

		public static void Detonate(SplashDamage damage, Vector3 position, GameObject source, out DamageHit[] hits, int layerMask = ~0, GameObject template = null)
		{
			if (damage == null)
			{
				hits = null;
				return;
			}

			if (template != null)
			{
				Spawner.InstantiateTemplate(template, position, Quaternion.identity, null);
			}

			damage.Apply(position, source, out hits, layerMask);
		}

		#endregion

		#region Editor-Only
#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, m_damage.innerRadius);

			Gizmos.color = Color.gray;
			Gizmos.DrawWireSphere(transform.position, m_damage.outerRadius);
		}

#endif
		#endregion
	}
}