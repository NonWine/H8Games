using UnityEngine;

// Opt-in trace for the hero's auto-attack loop, switched on from the player
// prefab's PlayerCombatInstaller. Each silent failure mode of the feature - never
// acquiring a target, acquiring one but never firing, firing at something outside
// the configured radius - shows up in these lines, so a bug report only needs the
// console output instead of a repro session.
public class HeroAutoAttackLogger
{
    private const string Prefix = "[HeroAutoAttack]";

    private readonly TargetingData targetingData;
    private readonly UnitStats unitStats;
    private readonly Transform heroTransform;
    private readonly bool isEnabled;

    private string lastSensingIdleReason;

    public bool IsEnabled => isEnabled;

    public HeroAutoAttackLogger(
        bool isEnabled,
        TargetingData targetingData,
        UnitStats unitStats,
        Transform heroTransform)
    {
        this.isEnabled = isEnabled;
        this.targetingData = targetingData;
        this.unitStats = unitStats;
        this.heroTransform = heroTransform;
    }

    // Static so the installer can report a broken prefab without resolving the
    // container mid-install, which would re-introduce the installer-order
    // coupling that BindData deliberately avoids.
    public static void LogMissingAttackOrigin()
    {
        Debug.LogError($"{Prefix} hero has no AttackPoint assigned on its BaseCombatUnitView - " +
                       "projectiles cannot be spawned.");
    }

    public static void LogMissingProjectilePrefab()
    {
        Debug.LogError($"{Prefix} hero has no ProjectilePrefab assigned on its BaseCombatUnitView - " +
                       "auto attacks will damage nothing.");
    }

    public void LogConfiguration()
    {
        if (!isEnabled)
            return;

        Debug.Log($"{Prefix} armed - detectionRadius={targetingData.DetectionRadius}, " +
                  $"retargetInterval={targetingData.RetargetInterval}, " +
                  $"targetLock={targetingData.TargetLockDuration}, " +
                  $"damage={unitStats.Damage}, cooldownRange={unitStats.AttackCooldownRange}");
    }

    public void LogTargetAcquired(ICombatTarget target)
    {
        if (!isEnabled)
            return;

        lastSensingIdleReason = null;
        Debug.Log($"{Prefix} target acquired: {DescribeTarget(target)}");
    }

    // Sensing runs every RetargetInterval, so one line per sweep would flood the
    // console. reasonKey is the stable category to throttle on - detail carries
    // the live numbers, which must stay out of the key or nothing throttles.
    public void LogSensingIdle(string reasonKey, string detail = null)
    {
        if (!isEnabled || reasonKey == lastSensingIdleReason)
            return;

        lastSensingIdleReason = reasonKey;
        Debug.Log($"{Prefix} no target - {detail ?? reasonKey}");
    }

    public void LogTargetLost()
    {
        if (!isEnabled)
            return;

        Debug.Log($"{Prefix} target lost - returning to idle");
    }

    public void LogShotFired(ICombatTarget target, float nextCooldown)
    {
        if (!isEnabled)
            return;

        Debug.Log($"{Prefix} shot fired at {DescribeTarget(target)}, nextCooldown={nextCooldown:F2}");
    }

    private string DescribeTarget(ICombatTarget target)
    {
        if (target == null)
            return "<null>";

        float distance = Vector3.Distance(heroTransform.position, target.transform.position);
        return $"'{target.transform.name}' at {distance:F2}m (radius {targetingData.DetectionRadius})";
    }
}
