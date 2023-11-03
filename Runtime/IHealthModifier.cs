using UnityEngine;

namespace ToolkitEngine.Health
{
    public interface IHealthModifier
    {
        bool TryGetFactor(DamageType damageType, GameObject target, out float value);
        bool TryModifyFactor(DamageType damageType, GameObject target, float delta);
        bool TrySetFactor(DamageType damageType, GameObject target, float value);

        public static float GetDelta(float delta, float factor)
        {
            return delta <= 0f
                ? delta * factor
                : delta / factor;
        }

        public const string RESISTANCE_FACTOR_TOOLTIP = "Factor to multiple incoming healing / damage of specified DamageType." + RESISTANCE_FACTOR_HINT_TOOLTIP;

        public const string RESISTANCE_FACTOR_HINT_TOOLTIP = "\n" +
            "    -1:\tHealed (or Half Damaged)\n" +
            "    0:\tImmune\n" +
            "    1:\tNormal\n" +
            "    2:\tDouble Damaged (or Half Healed)";
    }
}