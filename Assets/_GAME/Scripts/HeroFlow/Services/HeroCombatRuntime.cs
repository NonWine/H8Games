public sealed class HeroCombatRuntime : IHeroStateReader, IHeroUpgradeAccess
{
    private readonly UnitHealthHandler _unitHealthHandler;

    public HeroCombatRuntime(HeroStats runtimeStats, HeroUpgradeConfig runtimeUpgradeConfig)
    {
        RuntimeStats = runtimeStats;
        _unitHealthHandler = new UnitHealthHandler(runtimeStats.Combat.MaxHealth);
        UpgradeService = new HeroUpgradeService(runtimeStats, runtimeUpgradeConfig, _unitHealthHandler);
    }

    public event System.Action<float, float> HealthChanged
    {
        add => _unitHealthHandler.HealthChanged += value;
        remove => _unitHealthHandler.HealthChanged -= value;
    }

    public event System.Action Died
    {
        add => _unitHealthHandler.Died += value;
        remove => _unitHealthHandler.Died -= value;
    }

    public HeroStats RuntimeStats { get; }
    public HeroUpgradeService UpgradeService { get; }
    public bool IsAlive => _unitHealthHandler.IsAlive;
    public float CurrentHealth => _unitHealthHandler.CurrentHealth;
    public float MaxHealth => _unitHealthHandler.MaxHealth;

    public void ApplyDamage(float damage)
    {
        _unitHealthHandler.ApplyDamage(damage);
    }
}
