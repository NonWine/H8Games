using UnityEngine;

public interface IAllyTargetProvider
{
    ICombatTarget GetBestLivingAllyTarget(Vector3 worldPosition, TargetingData targetingData);
}
