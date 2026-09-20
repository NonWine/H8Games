using System.Collections.Generic;
using UnityEngine;

public class LevelRuntime : MonoBehaviour
{
    [SerializeField] private List<EnemyEncounterZoneView> zones = new();
    [SerializeField] private Transform startPoint;

    private readonly List<EnemyGroupViewController> uniqueGroups = new();

    public IReadOnlyList<EnemyEncounterZoneView> Zones => zones;
    public Transform StartPoint => startPoint;
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

    // Runs at level start and after a defeat, and both mean "put the whole level
    // back". Skipping groups that were already Cleared drained the level one
    // encounter at a time: those groups never came back, so a retry eventually had
    // nothing left to march at.
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
