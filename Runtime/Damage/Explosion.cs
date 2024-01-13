using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
	public class Explosion : MonoBehaviour
	{
		#region Fields

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

		#endregion

		#region Properties

		public SplashDamage damage => m_damage;
		public UnityEvent<Explosion> onDetonated => m_onDetonated;

		#endregion

		#region Methods

		[ContextMenu("Detonate")]
		public void Detonate()
		{
			m_damage.Apply(transform.position, !m_source.IsNull() ? m_source : null);
			m_spawner.Instantiate(transform.position, Quaternion.identity);

			m_onDetonated?.Invoke(this);
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