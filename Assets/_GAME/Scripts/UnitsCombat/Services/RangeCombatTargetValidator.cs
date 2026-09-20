using UnityEngine;

// Keeps a tracked target only while it stays inside the owner's detection
// radius, so an attacker drops back to idle when its victim walks away instead
// of firing across the whole level at a target it picked up point-blank.
public class RangeCombatTargetValidator : ICombatTargetValidator
{
    private readonly Transform ownerTransform;
    private readonly TargetingData targetingData;

    public RangeCombatTargetValidator(Transform ownerTransform, TargetingData targetingData)
    {
        this.ownerTransform = ownerTransform;
        this.targetingData = targetingData;
    }

    public bool IsValid(ICombatTarget target)
    {
        if (target == null)
        {
            return false;
        }

        Vector3 delta = target.transform.position - ownerTransform.position;
        delta.y = 0f;

        float detectionRadius = targetingData.DetectionRadius;
        return delta.sqrMagnitude <= detectionRadius * detectionRadius;
    }
}
