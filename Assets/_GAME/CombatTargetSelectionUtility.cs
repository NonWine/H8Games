using System.Collections.Generic;
using UnityEngine;

public static class CombatTargetSelectionUtility
{
    public static TTarget GetBestLivingTarget<TTarget>(
        IReadOnlyList<TTarget> targets,
        TTarget searcher,
        TargetingData targetingData)
        where TTarget : class, ITargetSelectionCandidate
    {
        Vector3 searcherPosition = searcher != null ? searcher.Position : default;
        return SelectBestTarget(targets, searcherPosition, targetingData, searcher);
    }

    public static TTarget SelectBestTarget<TTarget>(
        IReadOnlyList<TTarget> availableTargets,
        Vector3 searcherPosition,
        TargetingData targetingData,
        ITargetSelectionCandidate excludedTarget = null)
        where TTarget : class, ITargetSelectionCandidate
    {
        if (availableTargets == null || availableTargets.Count == 0 || targetingData == null)
        {
            return null;
        }

        TTarget bestTarget = null;
        float bestScore = float.MaxValue;
        float maxRangeSq = targetingData.DetectionRadius * targetingData.DetectionRadius;
        float reservationPenalty = targetingData.ReservationPenalty;

        for (int i = 0; i < availableTargets.Count; i++)
        {
            TTarget target = availableTargets[i];

            if (target == null || !target.IsAlive)
            {
                continue;
            }

            if (ReferenceEquals(target, excludedTarget))
            {
                continue;
            }

            Vector3 delta = target.Position - searcherPosition;
            delta.y = 0f;

            if (delta.sqrMagnitude > maxRangeSq)
            {
                continue;
            }

            float score = CalculateTargetScore(
                delta.sqrMagnitude,
                target.ReservationCount,
                reservationPenalty);

            if (score < bestScore)
            {
                bestTarget = target;
                bestScore = score;
            }
        }

        return bestTarget;
    }

    private static float CalculateTargetScore(
        float distanceSq,
        int reservationCount,
        float reservationPenalty)
    {
        float distanceScore = Mathf.Sqrt(distanceSq);
        float reservationScore = Mathf.Max(0, reservationCount) * Mathf.Max(0f, reservationPenalty);

        return distanceScore + reservationScore;
    }
}
