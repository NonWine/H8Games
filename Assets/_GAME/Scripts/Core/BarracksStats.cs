using System;
using UnityEngine;

[Serializable]
public class BarracksStats
{
    [Min(0.15f)] public float SpawnInterval = 2.5f;


    public BarracksStats()
    {
    }

    public BarracksStats(BarracksStats source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        SpawnInterval = source.SpawnInterval;
    }

    public void ReduceSpawnInterval(float amount)
    {
        SpawnInterval = Mathf.Max(0.15f, SpawnInterval - amount);
    }
}
