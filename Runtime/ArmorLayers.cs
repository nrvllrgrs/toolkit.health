using System.Collections.Generic;
using UnityEngine;
using static ToolkitEngine.Health.Armor;

namespace ToolkitEngine.Health
{
	[RequireComponent(typeof(HealthLayers))]
    public class ArmorLayers : MonoBehaviour, IHealthModifier
    {
		#region Fields

		[SerializeField]
		private ArmorLayer[] m_layers = new ArmorLayer[] { };

		private HealthLayers m_healthLayers;

		#endregion

		#region Properties

		public ArmorLayer[] groups => m_layers;

		#endregion

		#region Methods

		private void Awake()
		{
			m_healthLayers = GetComponent<HealthLayers>();
		}

		private void OnEnable()
		{
			m_healthLayers.onValueChanging.AddListener(ValueChanging);
		}

		private void OnDisable()
		{
			m_healthLayers.onValueChanging.RemoveListener(ValueChanging);
		}

		private void ValueChanging(HealthEventArgs e)
		{
			if (ReferenceEquals(e.hit.victim, m_healthLayers) && TryGetFactor(e.hit.damageType, null, out float factor))
			{
				e.postDamageFactor += factor - 1f;
			}
		}

		public bool TryGetLayer(out ArmorLayer layer)
		{
			if (m_healthLayers.index.Between(0, m_layers.Length - 1))
			{
				layer = m_layers[m_healthLayers.index];
				return true;
			}

			layer = null;
			return false;
		}

		#endregion

		#region IDamageResistance Methods

		public bool TryGetFactor(DamageType damageType, GameObject target, out float value)
        {
			if (!TryGetLayer(out ArmorLayer layer))
			{
				value = 0f;
				return false;
			}

			if (!layer.TryGetFactor(damageType, out value))
			{
				// Use fallback factor for group
				value = layer.factor;
			}
			return true;

		}

        public bool TryModifyFactor(DamageType damageType, GameObject target, float delta)
        {
			bool anyModified = false;
			foreach (var layer in m_layers)
			{
				anyModified |= layer.TryModifyFactor(damageType, delta);
			}

			return anyModified;
		}

        public bool TrySetFactor(DamageType damageType, GameObject target, float value)
        {
			return TrySetFactor(damageType, target, value, false);
		}

		public bool TrySetFactor(DamageType damageType, GameObject target, float value, bool setFallback)
		{
			if (!TryGetLayer(out ArmorLayer layer))
				return false;

			if (!layer.TrySetFactor(damageType, value) && setFallback)
			{
				// Use fallback factor for group
				layer.factor = value;
				return true;
			}

			return false;
		}

		#endregion

		#region Structures

		[System.Serializable]
		public class ArmorLayer
		{
			#region Fields

			[SerializeField, Tooltip("Fallback factor to multiple incoming healing / damage of unspecified DamageTypes in group." + IHealthModifier.RESISTANCE_FACTOR_HINT_TOOLTIP)]
			private float m_factor = 1f;

			[SerializeField]
			private List<Vulnerability> m_vulnerabilities;

			private Dictionary<DamageType, Vulnerability> m_map = null;

			#endregion

			#region Properties
			public float factor { get => m_factor; set => m_factor = value; }

			public Vulnerability[] vulnerabilities => m_vulnerabilities.ToArray();

			private Dictionary<DamageType, Vulnerability> map
			{
				get
				{
					if (m_map == null)
					{
						// Create a map for quick access to factors
						m_map = new Dictionary<DamageType, Vulnerability>();
						foreach (var vulnerability in m_vulnerabilities)
						{
							if (vulnerability?.damageType == null)
								continue;

							m_map.Add(vulnerability.damageType, vulnerability);
						}
					}
					return m_map;
				}
			}

			#endregion

			#region Methods

			public bool TryGetFactor(DamageType damageType, out float value)
			{
				if (damageType != null && map.TryGetValue(damageType, out Vulnerability resistance))
				{
					value = resistance.factor;
					return true;
				}

				value = 0f;
				return false;
			}

			public bool TrySetFactor(DamageType damageType, float value)
			{
				if (damageType != null && map.TryGetValue(damageType, out Vulnerability resistance))
				{
					resistance.factor = value;
					return true;
				}
				return false;
			}

			public bool TryModifyFactor(DamageType damageType, float delta)
			{
				if (damageType != null && map.TryGetValue(damageType, out Vulnerability resistance))
				{
					resistance.factor += delta;
					return true;
				}
				return false;
			}

			#endregion
		}

		#endregion
	}
}