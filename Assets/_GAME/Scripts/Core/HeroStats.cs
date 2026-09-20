using System;
using UnityEngine;

[Serializable]
public class HeroStats
{
    [SerializeField] private UnitStats combat = new();

    // Same split the enemy/soldier UnitConfig assets use (unitStats + targetingData):
    // combat holds damage/health/cooldown, targeting holds the auto-attack sensing
    // radius and retarget pacing, so the hero is tuned from one asset like every
    // other unit instead of from a field buried on the player prefab's installer.
    [SerializeField] private TargetingData targeting = new();

    [Min(0f)] public float PickupRadius = 2.5f;

    public UnitStats Combat => combat;

    public TargetingData Targeting => targeting;

    public HeroStats()
    {
    }

    public HeroStats(HeroStats source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        combat = source.combat != null ? new UnitStats(source.combat) : new UnitStats();
        targeting = source.targeting != null ? new TargetingData(source.targeting) : new TargetingData();
        PickupRadius = source.PickupRadius;
    }

}
