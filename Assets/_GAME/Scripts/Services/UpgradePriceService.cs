using UnityEngine;

public sealed class UpgradePriceService
{
    public int GetPrice(UpgradeDefinition definition, int currentLevel)
    {
        if (definition == null)
            return 0;

        int safeLevel = Mathf.Max(0, currentLevel);
        return Mathf.RoundToInt(definition.BaseCost * Mathf.Pow(definition.CostMultiplier, safeLevel));
    }

    public bool TryBuy(CurrencyService currencyService, UpgradeDefinition definition, int currentLevel, out int cost)
    {
        cost = GetPrice(definition, currentLevel);
        return currencyService != null && currencyService.TrySpend(cost);
    }
}
