public sealed class HeroCombatRuntime : IHeroStateReader, IHeroUpgradeAccess
{
    private readonly HealthService healthService;

    public HeroCombatRuntime(HeroStats runtimeStats, HeroUpgradeConfig runtimeUpgradeConfig)
    {
        RuntimeStats = runtimeStats;
        healthService = new HealthService(runtimeStats.Combat.MaxHealth);
        UpgradeService = new HeroUpgradeService(runtimeStats, runtimeUpgradeConfig, healthService);
    }

    public event System.Action<float, float> HealthChanged
    {
        add => healthService.HealthChanged += value;
        remove => healthService.HealthChanged -= value;
    }

    public event System.Action Died
    {
        add => healthService.Died += value;
        remove => healthService.Died -= value;
    }

    public HeroStats RuntimeStats { get; }
    public HeroUpgradeService UpgradeService { get; }
    public bool IsAlive => healthService.IsAlive;
    public float CurrentHealth => healthService.CurrentHealth;
    public float MaxHealth => healthService.MaxHealth;

    public void ApplyDamage(float damage)
    {
        healthService.ApplyDamage(damage);
    }
}
