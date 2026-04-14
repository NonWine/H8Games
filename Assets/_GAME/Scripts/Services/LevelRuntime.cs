using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelRuntime : MonoBehaviour
{
    [SerializeField] private List<EnemyEncounterZone> zones = new();

    private readonly List<EnemyGroupFacade> uniqueGroups = new();

    public IReadOnlyList<EnemyEncounterZone> Zones => zones;
    public IReadOnlyList<EnemyGroupFacade> Groups => uniqueGroups;

    private void Start()
    {
        RebuildGroups();
        uniqueGroups.ForEach(group => group.ResetRuntimeState());
    }

    private void OnValidate()
    {
        zones.Clear();
        GetComponentsInChildren(true, zones);
    }

    public void RebuildGroups()
    {
        uniqueGroups.Clear();

        for (int i = 0; i < zones.Count; i++)
        {
            EnemyGroupFacade group = zones[i].EnemyGroup;
            if (group == null || uniqueGroups.Contains(group))
                continue;

            uniqueGroups.Add(group);
        }
    }
    
}
