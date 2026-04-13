using System.Collections.Generic;

public sealed class HeroUpgradeService
{
    private readonly HeroStats stats;
    private readonly HeroUpgradeConfig config;
    private readonly HealthService healthService;
    private readonly Dictionary<UpgradeKind, int> levels = new();

    public HeroUpgradeService(HeroStats stats, HeroUpgradeConfig config, HealthService healthService = null)
    {
        this.stats = stats;
        this.config = config;
        this.healthService = healthService;
    }

    public int GetLevel(UpgradeKind kind)
    {
        return levels.TryGetValue(kind, out int level) ? level : 0;
    }

    public UpgradeDefinition GetDefinition(UpgradeKind kind)
    {
        return IsHeroUpgrade(kind) ? config?.GetDefinition(kind) : null;
    }

    public bool TryUpgrade(UpgradeKind kind, CurrencyService currencyService, UpgradePriceService upgradePriceService,
        out int spentCost)
    {
        spentCost = 0;
        UpgradeDefinition definition = config?.GetDefinition(kind);
        if (!IsHeroUpgrade(kind) || definition == null || GetLevel(kind) >= definition.MaxLevel)
            return false;

        int level = GetLevel(kind);
        if (!upgradePriceService.TryBuy(currencyService, definition, level, out spentCost))
            return false;

        Apply(kind);
        levels[kind] = level + 1;
        return true;
    }

    public void Apply(UpgradeKind kind)
    {
        UpgradeDefinition definition = config?.GetDefinition(kind);
        if (definition == null)
            return;

        switch (kind)
        {
            case UpgradeKind.HeroDamage:
                stats.Combat.IncreaseDamage(definition.Amount);
                break;
            case UpgradeKind.HeroMaxHealth:
                stats.Combat.IncreaseMaxHealth(definition.Amount);
                healthService?.IncreaseMaxHealth(definition.Amount, true);
                break;
            case UpgradeKind.HeroAttackRate:
                stats.Combat.ReduceAttackCooldown(definition.Amount);
                break;
        }
    }

    private static bool IsHeroUpgrade(UpgradeKind kind)
    {
        return kind == UpgradeKind.HeroDamage || kind == UpgradeKind.HeroMaxHealth ||
               kind == UpgradeKind.HeroAttackRate;
    }
}
