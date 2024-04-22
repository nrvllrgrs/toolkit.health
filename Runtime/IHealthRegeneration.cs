using UnityEngine;

namespace ToolkitEngine.Health
{
    public interface IHealthRegeneration
    {
        Transform transform { get; }
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