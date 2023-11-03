using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Health
{
    [RequireComponent(typeof(IHealth))]
    public class Armor : MonoBehaviour, IHealthModifier
    {
		#region Fields

		[SerializeField, Tooltip("Fallback factor to multiple incoming healing / damage of unspecified DamageTypes in group." + IHealthModifier.RESISTANCE_FACTOR_HINT_TOOLTIP)]
		private float m_factor = 1f;

		[SerializeField]
        private List<Vulnerability> m_vulnerabilities;

        private IHealth m_health;
        private Dictionary<DamageType, Vulnerability> m_map;

		#endregion

		#region Properties

        public float factor { get => m_factor; set => m_factor = value; }

		#endregion

		#region Methods

		private void Awake()
        {
            m_health = GetComponent<IHealth>();

            // Create a map for quick access to factors
            m_map = new Dictionary<DamageType, Vulnerability>();
            foreach (var vulnerability in m_vulnerabilities)
            {
                if (vulnerability?.damageType == null)
                    continue;

                m_map.Add(vulnerability.damageType, vulnerability);
            }
        }

        private void OnEnable()
        {
            m_health.onValueChanging.AddListener(ValueChanging);
        }

        private void OnDisable()
        {
			m_health.onValueChanging.RemoveListener(ValueChanging);
        }

        private void ValueChanging(HealthEventArgs e)
        {
            if (ReferenceEquals(e.hit.victim, m_health) && TryGetFactor(e.hit.damageType, null, out float factor))
            {
                e.postDamageFactor += factor - 1f;
            }
        }

        public bool TryGetFactor(DamageType damageType, GameObject target, out float value)
        {
            if (damageType != null && m_map.TryGetValue(damageType, out Vulnerability resistance))
            {
                value = resistance.factor;
                return true;
            }

            value = m_factor;
            return true;
        }

        public bool TryModifyFactor(DamageType damageType, GameObject target, float delta)
        {
            if (damageType != null && m_map.TryGetValue(damageType, out Vulnerability resistance))
            {
                resistance.factor += delta;
                return true;
            }
            return false;
        }

        public bool TrySetFactor(DamageType damageType, GameObject target, float value)
        {
			if (damageType != null && m_map.TryGetValue(damageType, out Vulnerability resistance))
			{
                resistance.factor = value;
				return true;
			}

            return false;
		}

        #endregion

        #region Structures

        [System.Serializable]
        public class Vulnerability
        {
            public DamageType damageType;

            [Tooltip(IHealthModifier.RESISTANCE_FACTOR_TOOLTIP)]
            public float factor = 1f;
        }

        #endregion
    }
}