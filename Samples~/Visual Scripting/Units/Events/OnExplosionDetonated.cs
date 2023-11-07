using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
	[UnitCategory("Events/Health")]
	[UnitTitle("On Detonated"), UnitSurtitle("Explosion")]
	public class OnExplosionDetonated : BaseEventUnit<Explosion>
	{
		protected override bool showEventArgs => false;
		public override Type MessageListenerType => typeof(OnExplosionDetonatedMessageListener);
	}
}