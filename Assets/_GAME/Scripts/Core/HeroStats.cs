using System;
using UnityEngine;

[Serializable]
public class HeroStats
{
    [SerializeField] private UnitStats combat = new();
    [Min(0f)] public float PickupRadius = 2.5f;

    public UnitStats Combat => combat;

    public HeroStats()
    {
    }

    public HeroStats(HeroStats source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        combat = source.combat != null ? new UnitStats(source.combat) : new UnitStats();
        PickupRadius = source.PickupRadius;
    }

    public HeroStats Clone()
    {
        return new HeroStats(this);
    }
}
