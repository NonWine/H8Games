using UnityEngine;

// One sensing rule for every ally: the closest live enemy inside this unit's own
// DetectionRadius, scored with the shared reservation penalty so two units
// standing side by side spread out over two victims instead of both piling onto
// the same one. No squad state takes part - a unit that can see something
// engages it, and the squad only decides where the formation walks.
public class NearestEnemyCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly TargetingData targetingData;
    private readonly IEnemyCandidateSource candidateSource;

    public NearestEnemyCombatTargetProvider(
        Transform ownerTransform,
        TargetingData targetingData,
        IEnemyCandidateSource candidateSource)
    {
        this.ownerTransform = ownerTransform;
        this.targetingData = targetingData;
        this.candidateSource = candidateSource;
    }

    public ICombatTarget GetTarget()
    {
        return CombatTargetSelectionUtility.SelectBestTarget(
            candidateSource.Candidates,
            ownerTransform.position,
            targetingData);
    }
}
