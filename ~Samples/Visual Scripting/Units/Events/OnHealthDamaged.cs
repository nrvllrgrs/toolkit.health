using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [UnitTitle("On Damaged"), UnitSurtitle("IDamageReceiver")]
    public class OnHealthDamaged : BaseHealthEventUnit
    {
        protected override bool showEventArgs => true;
        public override Type MessageListenerType => typeof(OnHealthDamagedMessageListener);
    }
}