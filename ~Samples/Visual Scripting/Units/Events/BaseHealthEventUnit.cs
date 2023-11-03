using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [UnitCategory("Events/Health")]
    public abstract class BaseHealthEventUnit : BaseEventUnit<HealthEventArgs>
    {
        protected override bool showEventArgs => false;
    }
}