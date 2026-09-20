using System.Collections.Generic;
using UnityEngine;

// Sensing itself is the shared per-unit rule; this only narrates it. The hero is
// the one unit whose silence a player notices immediately, so an empty sweep
// explains itself: nothing alive left, or nothing close enough.
public class HeroCombatTargetProvider : ICombatTargetProvider
{
    private readonly NearestEnemyCombatTargetProvider nearestEnemyProvider;
    private readonly IEnemyCandidateSource candidateSource;
    private readonly TargetingData targetingData;
    private readonly Transform heroTransform;
    private readonly HeroAutoAttackLogger logger;

    public HeroCombatTargetProvider(
        NearestEnemyCombatTargetProvider nearestEnemyProvider,
        IEnemyCandidateSource candidateSource,
        TargetingData targetingData,
        Transform heroTransform,
        HeroAutoAttackLogger logger)
    {
        this.nearestEnemyProvider = nearestEnemyProvider;
        this.candidateSource = candidateSource;
        this.targetingData = targetingData;
        this.heroTransform = heroTransform;
        this.logger = logger;
    }

    public ICombatTarget GetTarget()
    {
        ICombatTarget target = nearestEnemyProvider.GetTarget();

        if (target == null)
        {
            ReportEmptySweep();
        }

        return target;
    }

    private void ReportEmptySweep()
    {
        if (!logger.IsEnabled)
        {
            return;
        }

        IReadOnlyList<ITargetSelectionCandidate> candidates = candidateSource.Candidates;
        int liveCount = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            ITargetSelectionCandidate candidate = candidates[i];
            if (candidate == null || !candidate.IsAlive)
            {
                continue;
            }

            liveCount++;

            Vector3 delta = candidate.Position - heroTransform.position;
            delta.y = 0f;
            nearestDistance = Mathf.Min(nearestDistance, delta.magnitude);
        }

        if (liveCount == 0)
        {
            logger.LogSensingIdle(
                "no live enemies",
                $"no live enemies in the level ({candidates.Count} scanned)");
            return;
        }

        logger.LogSensingIdle(
            "out of range",
            $"{liveCount} live enemies but nearest is {nearestDistance:F2}m, " +
            $"outside detectionRadius {targetingData.DetectionRadius}");
    }
}
