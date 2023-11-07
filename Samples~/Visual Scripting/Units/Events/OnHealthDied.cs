using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [UnitTitle("On Died"), UnitSurtitle("IHealth")]
    public class OnHealthDied : BaseHealthEventUnit
    {
        public override Type MessageListenerType => typeof(OnHealthDiedMessageListener);
    }
}