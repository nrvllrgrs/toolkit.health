using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [UnitTitle("On Healed"), UnitSurtitle("IDamageReceiver")]
    public class OnHealthHealed : BaseHealthEventUnit
    {
        protected override bool showEventArgs => true;
        public override Type MessageListenerType => typeof(OnHealthHealedMessageListener);
    }
}