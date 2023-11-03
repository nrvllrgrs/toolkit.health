using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using ToolkitEngine.Health;
using UnityEngine;

using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace ToolkitEngine.AI.Tasks
{
	[TaskCategory("Toolkit/Health")]
	[TaskDescription("Returns success if health received damage; otherwise, returns failure.")]
	[TaskIcon("9bf305b3111512a40824b83884132a30", "9bf305b3111512a40824b83884132a30")]
	public class HasTakenDamage : Conditional
    {
		#region Fields

		[Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
		public SharedGameObject operatorGameObject;

		private IHealth m_health;
		private bool m_takenDamage = false;

		#endregion

		#region Methods

		public override void OnAwake()
		{
			m_health = GetDefaultGameObject(operatorGameObject.Value)
				.GetComponent<IHealth>();

			if (m_health != null)
			{
				m_health.onDamaged.AddListener(Health_Damaged);
			}
		}

		public override void OnBehaviorComplete()
		{
			if (m_health != null)
			{
				m_health.onDamaged.RemoveListener(Health_Damaged);
			}
		}

		public override TaskStatus OnUpdate()
		{
			return m_takenDamage
				? TaskStatus.Success
				: TaskStatus.Failure;
		}

		public override void OnEnd()
		{
			m_takenDamage = false;
		}

		private void Health_Damaged(HealthEventArgs e)
		{
			m_takenDamage = true;
		}

		#endregion
	}
}