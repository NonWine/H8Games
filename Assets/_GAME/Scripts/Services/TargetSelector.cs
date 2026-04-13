using System.Collections.Generic;
using UnityEngine;

public sealed class TargetSelector
{
    private readonly CombatOverlapScanner overlapScanner;

    public TargetSelector(int maxColliders = 32)
    {
        overlapScanner = new CombatOverlapScanner(maxColliders);
    }

    public ICombatTarget GetClosestEnemy(Transform origin, float radius, TeamId ownTeam, LayerMask layerMask)
    {
        if (origin == null)
            return null;

        List<ICombatTarget> candidates = overlapScanner.GetFilteredObjects<ICombatTarget>(
            origin.position,
            radius,
            layerMask,
            unit => unit != null && unit.IsAlive && unit.TeamId != TeamId.Neutral && unit.TeamId != ownTeam);

        ICombatTarget best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            ICombatTarget candidate = candidates[i];
            float distance = (candidate.transform.position - origin.position).sqrMagnitude;
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = candidate;
        }

        return best;
    }
}
