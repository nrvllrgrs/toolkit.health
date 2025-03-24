using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
	[AddComponentMenu("Health/Health Composite")]
	public class HealthComposite : MonoBehaviour, IHealth
	{
		#region Fields

		[SerializeField]
		private HealthGroup[] m_groups = new HealthGroup[] { };

		[SerializeField]
		private GroupAssignment m_groupAssignments = new();

#if UNITY_EDITOR
		[SerializeField]
		private bool m_visualize;
#endif

		private Dictionary<string, HealthGroup> m_map = new();
		private HealthGroup m_primaryGroup = null;
		private Collider m_primaryCollider = null;

		#endregion

		#region Events

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onValueChanging = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onValueChanged = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onHealed = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDamaged = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDying = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onDied = new UnityEvent<HealthEventArgs>();

		[SerializeField]
		private UnityEvent<HealthEventArgs> m_onResurrected = new UnityEvent<HealthEventArgs>();

		public event EventHandler Killed;

		#endregion

		#region Properties

		public HealthGroup[] groups => m_groups;
		public string[] names => m_groups.Select(x => x.name).ToArray();
		public GameObject[] objects => m_groupAssignments.Keys.ToArray();

		public HealthGroup primaryGroup
		{
			get
			{
				if (m_primaryGroup == null)
				{
					foreach (var group in m_groups)
					{
						if (!group.primary)
							continue;

						m_primaryGroup = group;
					}
				}
				return m_primaryGroup;
			}
		}

		protected Collider primaryCollider
		{
			get
			{
				if (m_primaryCollider == null)
				{
					var primaryName = primaryGroup.name;
					foreach (var assignment in m_groupAssignments)
					{
						if (Equals(assignment.Value, primaryName))
						{
							m_primaryCollider = assignment.Key.GetComponent<Collider>();
							break;
						}
					}
				}
				return m_primaryCollider;
			}
		}

		public float value
		{
			get => primaryGroup?.health.value ?? 0f;
			set
			{
				if (primaryGroup?.health != null)
				{
					primaryGroup.health.value = value;
				}
			}
		}

		public float maxValue => primaryGroup?.health.maxValue ?? 0f;

		public float normalizedValue => primaryGroup?.health.normalizedValue ?? 0f;

		public bool isDead => primaryGroup?.health.isDead ?? false;

		public UnityEvent<HealthEventArgs> onValueChanging => m_onValueChanging;

		public UnityEvent<HealthEventArgs> onValueChanged => m_onValueChanged;

		public UnityEvent<HealthEventArgs> onHealed => m_onHealed;

		public UnityEvent<HealthEventArgs> onDamaged => m_onDamaged;

		public UnityEvent<HealthEventArgs> onDying => m_onDying;

		public UnityEvent<HealthEventArgs> onDied => m_onDied;

		public UnityEvent<HealthEventArgs> onResurrected => m_onResurrected;

		#endregion

		#region Methods

		private void Awake()
		{
			UpdateGroups();
		}

		private void OnEnable()
		{
			foreach (var group in m_groups)
			{
				if (group.health == null)
					continue;

				group.health.onValueChanging.AddListener(Health_ValueChanging);
				group.health.onValueChanged.AddListener(Health_ValueChanged);
				group.health.onHealed.AddListener(Health_Healed);
				group.health.onDamaged.AddListener(Health_Damaged);
				group.health.onDying.AddListener(Health_Dying);
				group.health.onDied.AddListener(Health_Died);
				group.health.onResurrected.AddListener(Health_Resurrected);

				if (group.primary)
				{
					group.health.Killed += Health_Killed;
				}
			}
		}

		private void OnDisable()
		{
			foreach (var group in m_groups)
			{
				if (group.health == null)
					continue;

				group.health.onValueChanging.RemoveListener(Health_ValueChanging);
				group.health.onValueChanged.RemoveListener(Health_ValueChanged);
				group.health.onHealed.RemoveListener(Health_Healed);
				group.health.onDamaged.RemoveListener(Health_Damaged);
				group.health.onDying.RemoveListener(Health_Dying);
				group.health.onDied.RemoveListener(Health_Died);
				group.health.onResurrected.RemoveListener(Health_Resurrected);

				if (group.primary)
				{
					group.health.Killed -= Health_Killed;
				}
			}
		}

		public bool TryGetGroupName(GameObject obj, out string groupName)
		{
			return m_groupAssignments.TryGetValue(obj, out groupName);
		}

		public bool TryGetGroup(GameObject obj, out HealthGroup group)
		{
			group = null;
			return !obj.IsNull()
				&& m_groupAssignments.TryGetValue(obj, out string groupName)
				&& m_map.TryGetValue(groupName, out group);
		}

		public bool SetGroup(GameObject obj, string groupName)
		{
			if (!m_groupAssignments.ContainsKey(obj)
				|| !m_map.ContainsKey(groupName))
			{
				return false;
			}

			m_groupAssignments[obj] = groupName;
			return true;
		}

		public void UpdateGroups()
		{
			m_map.Clear();
			foreach (var group in m_groups)
			{
				if (m_map.ContainsKey(group.name))
					continue;

				m_map.Add(group.name, group);
			}
		}

		#endregion

		#region Callback Methods

		private void Health_ValueChanging(HealthEventArgs e) => m_onValueChanging?.Invoke(e);

		private void Health_ValueChanged(HealthEventArgs e) => m_onValueChanged?.Invoke(e);

		private void Health_Healed(HealthEventArgs e) => m_onHealed?.Invoke(e);

		private void Health_Damaged(HealthEventArgs e) => m_onDamaged?.Invoke(e);

		private void Health_Dying(HealthEventArgs e) => m_onDying?.Invoke(e);

		private void Health_Died(HealthEventArgs e) => m_onDied?.Invoke(e);

		private void Health_Resurrected(HealthEventArgs e) => m_onResurrected?.Invoke(e);

		private void Health_Killed(object sender, EventArgs e) => Killed?.Invoke(sender, e);

		#endregion

		#region IHealth Methods

		public void Apply(DamageHit damageInfo)
		{
			if (!TryGetGroup(damageInfo?.collider?.gameObject, out HealthGroup group))
				return;

			group.health?.Apply(damageInfo);
		}

		public void Damage(float delta, DamageType damageType = null)
		{
			Apply(new DamageHit(delta, damageType)
			{
				collider = primaryCollider
			});
		}

		public void Heal(float delta, DamageType damageType = null)
		{
			Apply(new DamageHit(-delta, damageType)
			{
				collider = primaryCollider
			});
		}

		#endregion

		#region Editor-Only Methods
#if UNITY_EDITOR

		public void UpdateGroupAssignments()
		{
			var keys = GetComponentsInChildren<Collider>(true)
				.Where(x => !x.isTrigger && ReferenceEquals(this, x.GetComponentInParent<IHealth>()))
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

		[Serializable]
		public class HealthGroup
		{
			#region Fields

			[SerializeField, Required, Tooltip("Name of group.")]
			private string m_name;

			[SerializeField]
			private Health m_health;

			[SerializeField]
			private bool m_primary;

#if UNITY_EDITOR
			[SerializeField]
			private Color m_color = Color.white;
#endif
			#endregion

			#region Properties

			public string name => m_name;
			public Health health => m_health;
			public bool primary => m_primary;

#if UNITY_EDITOR
			public Color color => m_color;
#endif
			#endregion
		}

		[Serializable]
		public class GroupAssignment : SerializableDictionary<GameObject, string> { }

		#endregion
	}
}