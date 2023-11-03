using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using static ToolkitEngine.Health.Armor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ToolkitEngine.Health
{
    [RequireComponent(typeof(IHealth))]
    public class ArmorComposite : MonoBehaviour, IHealthModifier
    {
        #region Fields

        [SerializeField]
        private ArmorGroup[] m_groups = new ArmorGroup[] { };

        [SerializeField]
        private GroupAssignment m_groupAssignments = new();

#if UNITY_EDITOR
		[SerializeField]
		private bool m_visualize;
#endif

		private IHealth m_health;
        private Dictionary<string, ArmorGroup> m_map = null;

        #endregion

		#region Properties

		public ArmorGroup[] groups => m_groups;
        public string[] names => m_groups.Select(x => x.name).ToArray();
        public GameObject[] objects => m_groupAssignments.Keys.ToArray();

        protected Dictionary<string, ArmorGroup> map
        {
            get
            {
                if (m_map == null)
                {
                    UpdateGroups();
                }
                return m_map;
            }
        }

        #endregion

        #region Methods

        private void Awake()
        {
            m_health = GetComponent<IHealth>();
            if (m_map != null)
            {
                UpdateGroups();
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
            if (e.hit?.collider == null)
                return;

            if (TryGetFactor(e.hit.damageType, e.hit.collider.gameObject, out ArmorGroup group, out float factor))
            {
                e.postDamageFactor += factor - 1f;
                group.onHit?.Invoke(e);
            }
        }

        public bool TryGetGroupName(GameObject obj, out string groupName)
        {
            return m_groupAssignments.TryGetValue(obj, out groupName);
        }

        public bool TryGetGroup(GameObject obj, out ArmorGroup group)
        {
            group = null;
            return !GameObjectExt.IsNull(obj)
                && m_groupAssignments.TryGetValue(obj, out string groupName)
                && map.TryGetValue(groupName, out group);
        }

        public bool TryGetGroup(string name, out ArmorGroup group)
        {
            if (string.IsNullOrWhiteSpace(name) || map.Count == 0)
            {
                group = null;
                return false;
            }

            return map.TryGetValue(name, out group);
        }

        public bool SetGroup(GameObject obj, string groupName)
        {
			if (!m_groupAssignments.ContainsKey(obj)
                || !map.ContainsKey(groupName))
            {
                return false;
            }

            m_groupAssignments[obj] = groupName;
            return true;
        }

        public void UpdateGroups()
        {
            if (m_map == null)
            {
                m_map = new();
            }

            m_map.Clear();
			foreach (var group in m_groups)
            {
                if (m_map.ContainsKey(group.name))
                    continue;

                m_map.Add(group.name, group);
            }
		}

        #endregion

        #region IDamageResistance Methods

        public bool TryModifyFactor(DamageType damageType, GameObject target, float delta)
        {
            bool anyModified = false; 
            foreach (var group in m_groups)
            {
                anyModified |= group.TryModifyFactor(damageType, delta);
            }

            return anyModified;
        }

		public bool TryGetFactor(DamageType damageType, GameObject target, out float value)
        {
            return TryGetFactor(damageType, target, out var group, out value);
        }

		public bool TryGetFactor(DamageType damageType, GameObject target, out ArmorGroup group, out float value)
        {
            if (!TryGetGroup(target, out group))
            {
                value = 0f;
                return false;
            }

            if (!group.TryGetFactor(damageType, out value))
            {
                // Use fallback factor for group
                value = group.factor;
            }
            return true;
        }

        public bool TrySetFactor(DamageType damageType, GameObject target, float value)
        {
			return TrySetFactor(damageType, target, value, false);
		}

        public bool TrySetFactor(DamageType damageType, GameObject target, float value, bool setFallback)
        {
			if (!TryGetGroup(target, out ArmorGroup group))
                return false;

			if (!group.TrySetFactor(damageType, value) && setFallback)
            {
                // Use fallback factor for group
				group.factor = value;
				return true;
			}

			return false;
		}

        #endregion

        #region Editor-Only Methods
#if UNITY_EDITOR

        public void UpdateGroupAssignments()
        {
            var keys = GetComponentsInChildren<Collider>(true)
				.Where(x => !x.isTrigger && ReferenceEquals(this, x.GetComponentInParent<IHealthModifier>()))
				.Select(x => x.gameObject)
                .Distinct();

            // Remove gameObjects that are no longer children
            var objectsToRemove = m_groupAssignments.Keys.Except(keys).ToArray();
            foreach (var key in objectsToRemove)
            {
                m_groupAssignments.Remove(key);
            }

            // Add gameObjects that are new children
            foreach (var key in keys.Except(m_groupAssignments.Keys))
            {
                m_groupAssignments.Add(key, null);
            }
        }

#endif
        #endregion

        #region Structures

        [System.Serializable]
        public class ArmorGroup
        {
            #region Fields

            [SerializeField, Required, Tooltip("Name of group.")]
            private string m_name;

            [SerializeField, Tooltip("Fallback factor to multiple incoming healing / damage of unspecified DamageTypes in group." + IHealthModifier.RESISTANCE_FACTOR_HINT_TOOLTIP)]
            private float m_factor = 1f;

#if UNITY_EDITOR
			[SerializeField]
			private Color m_color = Color.white;
#endif

			[SerializeField]
            private List<Vulnerability> m_vulnerabilities;

            private Dictionary<DamageType, Vulnerability> m_map = null;

            #endregion

            #region Fields

            [SerializeField]
            private UnityEvent<HealthEventArgs> m_onHit;

			#endregion

			#region Properties

			public string name => m_name;
            public float factor { get => m_factor; set => m_factor = value; }

#if UNITY_EDITOR
			public Color color => m_color;
#endif

			public Vulnerability[] vulnerabilities => m_vulnerabilities.ToArray();
            public UnityEvent<HealthEventArgs> onHit => m_onHit;

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

		[Serializable]
		public class GroupAssignment : SerializableDictionary<GameObject, string> { }

		#endregion
	}
}