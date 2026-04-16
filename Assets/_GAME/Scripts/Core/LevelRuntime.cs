using System.Collections.Generic;
using UnityEngine;

public class LevelRuntime : MonoBehaviour
{
    [SerializeField] private List<EnemyEncounterZoneView> zones = new();

    private readonly List<EnemyGroupViewController> uniqueGroups = new();

    public IReadOnlyList<EnemyEncounterZoneView> Zones => zones;
    public IReadOnlyList<EnemyGroupViewController> Groups => uniqueGroups;

    private void Start()
    {
        ResetRuntimeState();
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
            EnemyGroupViewController group = zones[i] != null ? zones[i].EnemyGroup : null;
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
            EnemyGroupViewController group = uniqueGroups[i];
            if (group == null)
                continue;

            group.ResetRuntimeState();
        }
    }
}
