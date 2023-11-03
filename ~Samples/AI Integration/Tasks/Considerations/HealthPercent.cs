using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using ToolkitEngine.Health;

namespace ToolkitEngine.AI.Tasks
{
	[TaskCategory("Toolkit/Health")] 
	public class HealthPercent : Consideration
    {
		#region Fields

		[SerializeField]
		private GameObjectPicker m_operator = new();

		[SerializeField]
		private UtilityCurve m_curve;

		#endregion

		#region Methods

		public override bool TryGetUtility(IUtilityAction action, GameObject target, out float score)
		{
			foreach (var t in m_operator.Pick(target))
			{
				if (t.TryGetComponent<IHealth>(out var health))
				{
					score = m_curve.Evaluate(health.normalizedValue);
					return true;
				}
			}

			score = 0f;
			return false;
		}

		public override bool TryGetUtility(IUtilityAction action, Vector3 position, Quaternion rotation, out float score)
		{
			throw new System.NotSupportedException();
		}

		#endregion
	}
}