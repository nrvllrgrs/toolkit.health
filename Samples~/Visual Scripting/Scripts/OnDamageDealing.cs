using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
	[UnitTitle("On Damage Dealing")]
	[UnitCategory("Events/Health")]
	public class OnDamageDealing : BaseEventUnit<HealthEventArgs>
	{
		public override Type MessageListenerType => typeof(OnDamageDealingMessageListener);
	}
}