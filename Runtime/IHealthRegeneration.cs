using UnityEngine;

namespace ToolkitEngine.Health
{
    public interface IHealthRegeneration
    {
        Transform transform { get; }
        float regenerationRate { get; set; }
    }
}