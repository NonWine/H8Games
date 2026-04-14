using System.Collections.Generic;
using UnityEngine;

public class LevelRuntime : MonoBehaviour
{
    [SerializeField] private bool autoCollectZonesFromChildren = true;
    [SerializeField] private List<EnemyEncounterZone> zones = new();

    private readonly List<EnemyGroupFacade> uniqueGroups = new();

    public IReadOnlyList<EnemyEncounterZone> Zones => zones;
    public IReadOnlyList<EnemyGroupFacade> Groups => uniqueGroups;

    private void Awake()
    {
        if (autoCollectZonesFromChildren)
        {
            zones.Clear();
            GetComponentsInChildren(true, zones);
        }

        RebuildGroups();
    }

    public void RebuildGroups()
    {
        uniqueGroups.Clear();

        for (int i = 0; i < zones.Count; i++)
        {
            EnemyGroupFacade group = zones[i] != null ? zones[i].EnemyGroup : null;
            if (group == null || uniqueGroups.Contains(group))
                continue;

            uniqueGroups.Add(group);
        }
    }

    public void ResetRuntimeState()
    {
        RebuildGroups();

        for (int i = 0; i < uniqueGroups.Count; i++)
        {
            EnemyGroupFacade group = uniqueGroups[i];
            if (group == null)
                continue;

            group.ResetRuntimeState();
        }
    }
}
