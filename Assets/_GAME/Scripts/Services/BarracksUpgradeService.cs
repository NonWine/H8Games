using System.Collections.Generic;

public sealed class BarracksUpgradeService
{
    private readonly BarracksStats stats;
    private readonly Dictionary<UpgradeKind, int> levels = new();

    public BarracksUpgradeService(BarracksStats stats)
    {
        this.stats = stats;
    }

    public int GetLevel(UpgradeKind kind)
    {
        return levels.TryGetValue(kind, out int level) ? level : 0;
    }

    public bool TryUpgrade(UpgradeKind kind, UpgradeDefinition definition, CurrencyService currencyService,
        UpgradePriceService upgradePriceService, out int spentCost)
    {
        spentCost = 0;
        if (!IsBarracksUpgrade(kind) || definition == null || GetLevel(kind) >= definition.MaxLevel)
            return false;

        int level = GetLevel(kind);
        if (!upgradePriceService.TryBuy(currencyService, definition, level, out spentCost))
            return false;

        Apply(kind, definition);
        levels[kind] = level + 1;
        return true;
    }

    public void Apply(UpgradeKind kind, UpgradeDefinition definition)
    {
        if (definition == null)
            return;

        switch (kind)
        {
            case UpgradeKind.BarracksSpawnSpeed:
                stats.ReduceSpawnInterval(definition.Amount);
                break;
            case UpgradeKind.BarracksUnitHealth:
                stats.MultiplyUnitHealth(1f + definition.Amount);
                break;
            case UpgradeKind.BarracksUnitDamage:
                stats.MultiplyUnitDamage(1f + definition.Amount);
                break;
        }
    }

    private static bool IsBarracksUpgrade(UpgradeKind kind)
    {
        return kind == UpgradeKind.BarracksSpawnSpeed || kind == UpgradeKind.BarracksUnitHealth ||
               kind == UpgradeKind.BarracksUnitDamage;
    }
}
