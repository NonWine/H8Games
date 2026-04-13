using System;
using UnityEngine;

[Serializable]
public class UpgradeDefinition
{
    [Min(0)] public int BaseCost = 20;
    [Min(1f)] public float CostMultiplier = 1.5f;
    [Min(1)] public int MaxLevel = 5;
    [Min(0f)] public float Amount = 5f;

    public UpgradeDefinition()
    {
    }

    public UpgradeDefinition(UpgradeDefinition source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        BaseCost = source.BaseCost;
        CostMultiplier = source.CostMultiplier;
        MaxLevel = source.MaxLevel;
        Amount = source.Amount;
    }
}
