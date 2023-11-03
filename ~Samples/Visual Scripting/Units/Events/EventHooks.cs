namespace ToolkitEngine.Health.VisualScripting
{
    public static class EventHooks
    {
        // Health
        public const string OnHealthValueChanged = nameof(OnHealthValueChanged);
        public const string OnHealthHealed = nameof(OnHealthHealed);
        public const string OnHealthDamaged = nameof(OnHealthDamaged);
        public const string OnHealthDying = nameof(OnHealthDying);
        public const string OnHealthDied = nameof(OnHealthDied);
        public const string OnHealthResurrected = nameof(OnHealthResurrected);

        // Explosion
        public const string OnExplosionDetonated = nameof(OnExplosionDetonated);
    }
}