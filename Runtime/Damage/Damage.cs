using UnityEngine;

namespace ToolkitEngine.Health
{
	[System.Serializable]
    public class Damage
    {
		#region Fields

		[SerializeField]
		protected DamageType m_damageType;

		[SerializeField]
		protected float m_value;

		#endregion

		#region Properties

		public float value { get => m_value; set => m_value = value; }
		public DamageType damageType { get => m_damageType; set => m_damageType = value; }

		#endregion

		#region Constructors

		public Damage()
		{ }

		public Damage(float value, DamageType damageType)
		{
			m_value = value;
			m_damageType = damageType;
		}

		#endregion
	}

	public class DamageHit
	{
		#region Fields

		private Vector3 m_normal = Vector3.zero;
		private float m_distance = -1f;

		#endregion

		#region Properties

		/// <summary>
		/// Amount of damage dealt
		/// </summary>
		public float value { get; set; }

		/// <summary>
		/// Type of damage dealt
		/// </summary>
		public DamageType damageType { get; set; }

		/// <summary>
		/// Source of damage
		/// </summary>
		public GameObject source { get; set; }

		/// <summary>
		/// Victim of damage
		/// </summary>
		public IDamageReceiver victim { get; set; }

		/// <summary>
		/// Collider hit by damage
		/// </summary>
		public Collider collider { get; set; }

		/// <summary>
		/// Position where damage originated from
		/// </summary>
		public Vector3 origin { get; set; }

		/// <summary>
		/// Position where damage contacted victim
		/// </summary>
		public Vector3 contact { get; set; }

		public Vector3 normal
		{
			get
			{
				if (m_normal == Vector3.zero)
				{
					m_normal = (origin - contact).normalized;
				}
				return m_normal;
			}
		}

		/// <summary>
		/// Distance between origin and point
		/// </summary>
		public float distance
		{
			get
			{
				if (m_distance < 0f)
				{
					m_distance = Vector3.Distance(origin, contact);
				}
				return m_distance;
			}
			set => m_distance = value;
		}

		#endregion

		#region Constructors

		public DamageHit(float value, DamageType damageType)
		{
			this.value = value;
			this.damageType = damageType;
		}

		public DamageHit(Damage damage)
			: this(damage, false)
		{ }

		public DamageHit(Damage damage, bool continuous)
		{
			if (damage == null)
				return;

			value = -damage.value;
			if (continuous)
			{
				value *= Time.deltaTime;
			}

			damageType = damage.damageType;
		}

		public DamageHit(float value, DamageType damageType, DamageHit other)
			: this(value, damageType)
		{
			Copy(other);
		}

		public DamageHit(DamageHit other)
			: this(other.value, other.damageType)
		{
			Copy(other);
		}

		#endregion

		#region Methods

		private void Copy(DamageHit other)
		{
			source = other.source;
			victim = other.victim;
			collider = other.collider;
			origin = other.origin;
			contact = other.contact;
		}

		#endregion
	}
}