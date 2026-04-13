using System.Collections.Generic;

public sealed class BarracksUpgradeService
{
    private readonly BarracksStats stats;
    private readonly BarracksUpgradeConfig config;
    private readonly Dictionary<UpgradeKind, int> levels = new();

    public BarracksUpgradeService(BarracksStats stats, BarracksUpgradeConfig config)
    {
        this.stats = stats;
        this.config = config;
    }

    public int GetLevel(UpgradeKind kind)
    {
        return levels.TryGetValue(kind, out int level) ? level : 0;
    }

    public UpgradeDefinition GetDefinition(UpgradeKind kind)
    {
        return IsBarracksUpgrade(kind) ? config?.GetDefinition(kind) : null;
    }

    public bool TryUpgrade(UpgradeKind kind, CurrencyService currencyService, UpgradePriceService upgradePriceService,
        out int spentCost)
    {
        spentCost = 0;
        UpgradeDefinition definition = config?.GetDefinition(kind);
        if (!IsBarracksUpgrade(kind) || definition == null || GetLevel(kind) >= definition.MaxLevel)
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
    }

    private static bool IsBarracksUpgrade(UpgradeKind kind)
    {
        return kind == UpgradeKind.BarracksSpawnSpeed || kind == UpgradeKind.BarracksUnitHealth ||
               kind == UpgradeKind.BarracksUnitDamage;
    }
}
