using UnityEngine;

public class DefaultCombatTargetValidator : ICombatTargetValidator
{
    public bool IsValid(ICombatTarget target)
    {
        if (target == null || !target.IsAlive)
        {
            return false;
        }

        if (target is Component targetComponent)
        {
            return targetComponent.gameObject.activeInHierarchy;
        }

        return true;
    }
}
