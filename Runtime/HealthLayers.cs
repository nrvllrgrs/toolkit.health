using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace ToolkitEngine.Health
{
	[AddComponentMenu("Health/Health Layers")]
	public class HealthLayers : MonoBehaviour, IHealth, IPoolItemRecyclable
    {
        #region Fields

        [SerializeField]
        private Layer[] m_layers;

        private int m_index;
        private bool m_isDead = false;

        private Dictionary<IHealth, int> m_indexMap = new();
        private Dictionary<string, int> m_nameToIndexMap = new();

        /// <summary>
        /// Indicates whether layer is dead when recycled
        /// </summary>
        private Dictionary<Layer, bool> m_layerMap = new();

        #endregion

        #region Events

        [SerializeField]
        private UnityEvent<HealthEventArgs> m_onValueChanging;

        [SerializeField]
        private UnityEvent<HealthEventArgs> m_onValueChanged;

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onHealed;

		[SerializeField]
        private UnityEvent<HealthEventArgs> m_onDamaged;

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDying;

		[SerializeField]
        private UnityEvent<HealthEventArgs> m_onDied;

        [SerializeField]
        private UnityEvent<HealthEventArgs> m_onResurrected;

        public event EventHandler Killed;

        #endregion

        #region Properties

        public bool isDead => m_isDead;

        public float value
        {
            get => TryGetActiveLayer(out Layer layer) ? layer.health.value : 0f;
            set
            {
                if (TryGetActiveLayer(out Layer layer) && layer?.health != null)
                {
                    layer.health.value = value;
                }
            }
        }
        public float maxValue => TryGetActiveLayer(out Layer layer) ? layer.health.maxValue : 0f;
        public float normalizedValue => TryGetActiveLayer(out Layer layer) ? layer.health.normalizedValue : 0f;

        public Layer[] layers => m_layers;
        public int index => m_index;

        public UnityEvent<HealthEventArgs> onValueChanging => m_onValueChanging;
        public UnityEvent<HealthEventArgs> onValueChanged => m_onValueChanged;
		public UnityEvent<HealthEventArgs> onHealed => m_onHealed;
		public UnityEvent<HealthEventArgs> onDamaged => m_onDamaged;
		public UnityEvent<HealthEventArgs> onDying => m_onDying;
		public UnityEvent<HealthEventArgs> onDied => m_onDied;
        public UnityEvent<HealthEventArgs> onResurrected => m_onResurrected;

        #endregion

        #region Methods

        public void Recycle()
        {
            m_isDead = false;

            m_index = 0;
            foreach (var layer in m_layers)
            {
                if (m_layerMap.TryGetValue(layer, out bool isDead) && isDead)
                {
                    ++m_index;
                }
            }
        }

        private void Awake()
        {
            int index = 0;
            foreach (var layer in m_layers)
            {
                m_indexMap.Add(layer.health, index);
                m_nameToIndexMap.Add(layer.name, index++);

                // Advance active layer if layer is dead
                if (layer.health.isDead)
                {
                    ++m_index;
                }

                m_layerMap.Add(layer, layer.health.isDead);
            }
        }

        private void OnEnable()
        {
            foreach (var layer in m_layers)
            {
                layer.health.onValueChanging.AddListener(Health_ValueChanging);
                layer.health.onValueChanged.AddListener(Health_ValueChanged);
                layer.health.onHealed.AddListener(Health_Healed);
                layer.health.onDamaged.AddListener(Health_Damaged);
                layer.health.onDying.AddListener(Health_Dying);
                layer.health.onDied.AddListener(Health_Died);
                layer.health.onResurrected.AddListener(Health_Resurrected);
            }
        }

        private void OnDisable()
        {
            foreach (var layer in m_layers)
            {
				layer.health.onValueChanging.RemoveListener(Health_ValueChanging);
				layer.health.onValueChanged.RemoveListener(Health_ValueChanged);
				layer.health.onHealed.RemoveListener(Health_Healed);
				layer.health.onDamaged.RemoveListener(Health_Damaged);
				layer.health.onDying.RemoveListener(Health_Dying);
				layer.health.onDied.RemoveListener(Health_Died);
                layer.health.onResurrected.RemoveListener(Health_Resurrected);
            }
        }

        private void SetIsDead(bool value)
        {
            // No change, skip
            if (m_isDead == value)
                return;

            m_isDead = value;
            if (m_isDead)
            {
                Killed?.Invoke(gameObject, EventArgs.Empty);
            }
        }

        #endregion

        #region IHealth Callbacks

        private void Health_ValueChanging(HealthEventArgs e)
        {
            m_onValueChanging?.Invoke(e);
         }

        private void Health_ValueChanged(HealthEventArgs e)
        {
			m_onValueChanged?.Invoke(e);
		}

		private void Health_Healed(HealthEventArgs e)
        {
			m_onHealed?.Invoke(e);
		}

        private void Health_Damaged(HealthEventArgs e)
        {
			m_onDamaged?.Invoke(e);
		}

		private void Health_Dying(HealthEventArgs e)
        {
            m_onDying?.Invoke(e);
        }

		private void Health_Died(HealthEventArgs e)
        {
            if (m_indexMap.TryGetValue(e.hit.victim as IHealth, out int index))
            {
                var current = m_layers[index];

                // Advance index to next living health
                int i = index + 1;
                for (; i < m_layers.Length; ++i)
                {
                    if (!m_layers[i].health.isDead)
                    {
                        m_index = i;
                        if (current.overflowDamage)
                        {
                            // Will find which layer to hit with specified damage type in method
                            Damage(e.hit.value - e.delta, e.hit.damageType);
                        }
                        break;
                    }
                }

				SetIsDead(i >= m_layers.Length);
			}

			// Notify after index has been updated
			m_onDied?.Invoke(e);
		}

        private void Health_Resurrected(HealthEventArgs e)
        {
            if (m_indexMap.TryGetValue(e.hit.victim as IHealth, out int index))
            {
                // Higher priorty layer has resurrected
                if (index < m_index)
                {
                    m_index = index;
                }

                SetIsDead(false);
            }

            // Notify after index has been updated
			m_onResurrected?.Invoke(e);
		}

        #endregion

        #region IHealth Methods

        public void Apply(DamageHit hit)
        {
			if (!TryGetHitLayer(hit.damageType, m_index, out Layer layer))
				return;

			// Taking damage and not dead...
			if (hit.value < 0 && !isDead)
			{
				foreach (var previous in GetPreviousLayers())
				{
					previous.health.UnpauseRegeneration();
				}
			}

			// HealthLayer needs to be the victim so that its armor is applied
			hit.victim = this;
			layer.health.Apply(hit);
		}

        public void Damage(float delta, DamageType damageType = null)
        {
            if (!TryGetHitLayer(damageType, m_index, out Layer layer))
                return;

            Damage(delta, damageType, layer);
        }

        public void Damage(float delta, string layerName, DamageType damageType = null)
        {
			if (!m_nameToIndexMap.TryGetValue(layerName, out int index))
				return;

			if (!TryGetHitLayer(damageType, index, out var layer))
				return;

			Damage(delta, damageType, layer);
        }

        private void Damage(float delta, DamageType damageType, Layer layer)
        {
            var hit = new DamageHit(-delta, damageType);
            hit.victim = this;
            layer.health.Apply(hit);

            if (!isDead)
            {
                foreach (var previous in GetPreviousLayers())
                {
                    previous.health.UnpauseRegeneration();
                }
            }
        }

        public void Heal(float delta, DamageType damageType = null)
        {
            if (!TryGetHitLayer(damageType, m_index, out Layer layer))
                return;

            Heal(delta, damageType, layer);
        }

        public void Heal(float delta, string layerName, DamageType damageType = null)
        {
            if (!m_nameToIndexMap.TryGetValue(layerName, out int index))
                return;

            if (!TryGetHitLayer(damageType, index, out var layer))
                return;

            Heal(delta, damageType, layer);
        }

        private void Heal(float delta, DamageType damageType, Layer layer)
        {
			var hit = new DamageHit(delta, damageType);
			hit.victim = this;
			layer.health.Apply(hit);
        }

        private Layer[] GetPreviousLayers()
        {
            return m_index > 0
                ? m_layers.Take(m_index).ToArray()
                : new Layer[] { };
        }

        public bool TryGetActiveLayer(out Layer layer)
        {
            return TryGetLayer(m_index, out layer);
        }

        private bool TryGetLayer(int index, out Layer layer)
        {
			layer = m_index.Between(0, m_layers.Length - 1)
				? m_layers[index]
				: null;
			return layer != null;
		}

        private bool TryGetHitLayer(DamageType damageType, int index, out Layer layer)
        {
			if (!TryGetLayer(index, out layer))
				return false;

			// Advance to next layer, if damage type is ignored by this layer
			if (layer.health.isDead
                || (damageType != null && layer.ignoredDamageTypes.Contains(damageType)))
			{
				return TryGetHitLayer(damageType, index + 1, out layer);
			}

            return true;
		}

		#endregion

		#region Editor-Only
#if UNITY_EDITOR

		[ContextMenu("Damage")]
		private void DebugDamage()
		{
			Damage(1f);
		}

		[ContextMenu("Heal")]
		private void DebugHeal()
		{
			Heal(1f);
		}

#endif
		#endregion

		#region Structures

		[Serializable]
        public class Layer
        {
            [SerializeField, Required]
            private string m_name;

            public Health health;

			[Tooltip("List of DamageTypes that are ignored by this layer (i.e. damage advances to next layer)")]
			public DamageType[] ignoredDamageTypes;

			[Tooltip("Indicates whether overkill value will damage the next layer.")]
            public bool overflowDamage;

			public string name => m_name;
        }

        #endregion
    }
}