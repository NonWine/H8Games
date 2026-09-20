using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyGroupDetector
{
    private readonly SquadRootView _squadRootView;

    [Inject]
    public EnemyGroupDetector(SquadRootView squadRootView)
    {
        this._squadRootView = squadRootView;
    }

    public EnemyGroupViewController FindNearestValidGroup(LevelRuntime levelRuntime)
    {
        if (levelRuntime == null || _squadRootView == null)
            return null;

        List<EnemyGroupViewController> groupFacades = new List<EnemyGroupViewController>();
        levelRuntime.RebuildGroups();
        groupFacades.AddRange(levelRuntime.Groups);

        EnemyGroupViewController nearestGroup = null;
        float nearestSqrDistance = float.MaxValue;
        Vector3 squadPosition = _squadRootView.transform.position;

        for (int i = 0; i < groupFacades.Count; i++)
        {
            EnemyGroupViewController group = groupFacades[i];
            if (group == null || group.State == EnemyGroupState.Cleared)
                continue;

            // A group only counts as a destination while someone in it is still
            // standing. Cleared alone is not enough: the flag is raised by the
            // death event, so a group emptied any other way - reset, an authored
            // group with no enemies - would otherwise be handed back forever and
            // the squad would re-target it every frame.
            if (!group.HasAliveMembers)
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
