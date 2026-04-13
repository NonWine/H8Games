using System;
using UnityEngine;

public sealed class AttackService
{
    private readonly Func<float> damageProvider;
    private readonly Func<float> cooldownProvider;
    private readonly Func<Vector3> attackOriginProvider;
    private float cooldownRemaining;

    public AttackService(Func<float> damageProvider, Func<float> cooldownProvider, Func<Vector3> attackOriginProvider)
    {
        this.damageProvider = damageProvider;
        this.cooldownProvider = cooldownProvider;
        this.attackOriginProvider = attackOriginProvider;
    }

    public bool Tick(float deltaTime, IDamageable target)
    {
        cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);

        if (target == null || !target.IsAlive || cooldownRemaining > 0f)
            return false;

        cooldownRemaining = Mathf.Max(0.05f, cooldownProvider?.Invoke() ?? 0.25f);
        target.GetDamage(Mathf.Max(0f, damageProvider?.Invoke() ?? 0f), attackOriginProvider?.Invoke() ?? Vector3.zero);
        return true;
    }

    public void ResetCooldown()
    {
        cooldownRemaining = 0f;
    }
}
