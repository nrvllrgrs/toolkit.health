using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
	[UnitTitle("On Damage Dealt")]
	[UnitCategory("Events/Health")]
	public class OnDamageDealt : BaseEventUnit<HealthEventArgs>
	{
		public override Type MessageListenerType => typeof(OnDamageDealtMessageListener);
	}
}