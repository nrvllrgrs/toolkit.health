using UnityEngine.Events;

namespace ToolkitEngine.Health
{
    public interface IHealth : IDamageReceiver, IKillable
    {
        float value { get; }
        float maxValue { get; }
        float normalizedValue { get; }
		UnityEvent<HealthEventArgs> onDying { get; }
		UnityEvent<HealthEventArgs> onDied { get; }
        UnityEvent<HealthEventArgs> onResurrected { get; }
    }
}