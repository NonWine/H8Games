using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyGroupDetector
{
    private readonly SquadRoot squadRoot;

    [Inject]
    public EnemyGroupDetector(SquadRoot squadRoot)
    {
        this.squadRoot = squadRoot;
    }

    public EnemyGroupFacade FindNearestValidGroup(LevelRuntime levelRuntime)
    {
        if (levelRuntime == null || squadRoot == null)
            return null;

        List<EnemyGroupFacade> groupFacades = new List<EnemyGroupFacade>();
        levelRuntime.RebuildGroups();
        groupFacades.AddRange(levelRuntime.Groups);

        EnemyGroupFacade nearestGroup = null;
        float nearestSqrDistance = float.MaxValue;
        Vector3 squadPosition = squadRoot.transform.position;

        for (int i = 0; i < groupFacades.Count; i++)
        {
            EnemyGroupFacade group = groupFacades[i];
            if (group.State == EnemyGroupState.Cleared)
                continue;

            Vector3 delta = group.EngagePointPosition - squadPosition;
            delta.y = 0f;
            float sqrDistance = delta.sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance)
                continue;

            nearestGroup = group;
            nearestSqrDistance = sqrDistance;
        }

        return nearestGroup;
    }
}
