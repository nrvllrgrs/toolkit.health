using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
    public interface IHealthRegeneration
    {
        Transform transform { get; }
		bool canRegenerate { get; }
		float regenerateDelay { get; set; }
		float degenerateDelay { get; set; }
		DamageType[] regenerateDamageTypes { get; }
		UnityEvent onRegenerationChanged { get; }
		float GetRegenerationRate();
        float GetRegenerationRate(DamageType damageType);
        void SetRegenerationRate(float value);
        void SetRegenerationRate(DamageType damageType, float value);
        void ModifyRegenerationRate(float value);
        void ModifyRegenerationRate(DamageType damageType, float value);
        void PauseRegeneration();
        void UnpauseRegeneration();
	}
}