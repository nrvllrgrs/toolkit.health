using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [UnitTitle("On Resurrected"), UnitSurtitle("IHealth")]
    public class OnHealthResurrected : BaseHealthEventUnit
    {
        public override Type MessageListenerType => typeof(OnHealthResurrectedMessageListener);
    }
}