using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using ToolkitEngine.Health;
using UnityEngine;

using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace ToolkitEngine.AI.Tasks
{
	[TaskCategory("Toolkit/Health")]
	[TaskDescription("Returns success if health received amount of damage; otherwise, returns failure.")]
	[TaskIcon("9bf305b3111512a40824b83884132a30", "9bf305b3111512a40824b83884132a30")]
	public class HasAccruedDamage : ToolkitConditional<IHealth>
	{
		#region Enumerators

		public enum ValueType
		{
			Constant,
			Percent,
		}

		#endregion

		#region Fields

		[SerializeField]
		private ValueType m_valueType;

		[SerializeField]
		private SharedFloat m_value = 1f;

		[SerializeField]
		private SharedFloat m_accruedValue;

		[SerializeField]
		private SharedBool m_resetOnSuccess = true;

		#endregion

		#region Methods

		public override void OnAwake()
		{
			base.OnAwake();
			if (m_component != null)
			{
				m_component.onDamaged.AddListener(Health_Damaged);
			}
		}

		public override void OnBehaviorComplete()
		{
			if (m_component != null)
			{
				m_component.onDamaged.RemoveListener(Health_Damaged);
			}
		}

		public override TaskStatus OnUpdate()
		{
			switch (m_valueType)
			{
				case ValueType.Constant:
					if (m_accruedValue.Value < m_value.Value)
						return TaskStatus.Failure;

					break;

				case ValueType.Percent:
					if (m_accruedValue.Value / m_component.maxValue < m_value.Value)
						return TaskStatus.Failure;

					break;

			}

			if (m_resetOnSuccess.Value)
			{
				m_accruedValue.Value = 0f;
			}
			return TaskStatus.Success;
		}

		private void Health_Damaged(HealthEventArgs e)
		{
			m_accruedValue.Value += -e.delta;
		}

		#endregion
	}
}