using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [UnitTitle("On Value Changed"), UnitSurtitle("IDamageReceiver")]
    public class OnHealthValueChanged : BaseHealthEventUnit
    {
        protected override bool showEventArgs => true;
        public override Type MessageListenerType => typeof(OnHealthValueChangedMessageListener);
    }
}