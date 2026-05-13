using UnityEngine;

public struct UnitDamageData
{
    public bool HasImpact;
    public Vector3 ImpactPoint;
    public Vector3 ImpactDirection;
    public float ImpactStrength;

    public static UnitDamageData FromHitData(HitData hitData, Vector3 targetPosition, float strength = 5f)
    {
        var direction = (targetPosition - hitData.sourceWorldPosition).normalized;

        return new UnitDamageData
        {
            HasImpact = true,
            ImpactPoint = targetPosition,
            ImpactDirection = direction,
            ImpactStrength = strength
        };
    }
}
