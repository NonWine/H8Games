using UnityEngine;

public class UnitAttackAgentHandler
{
    private readonly AttackRuntimeModel attackData;

    public UnitAttackAgentHandler(AttackRuntimeModel attackData)
    {
        this.attackData = attackData;
    }

    public bool Tick(float deltaTime)
    {
        attackData.CooldownRemaining = Mathf.Max(0f, attackData.CooldownRemaining - deltaTime);

        if (attackData.CooldownRemaining > 0f)
        {
            return false;
        }

        attackData.CooldownRemaining = Mathf.Max(0.05f, attackData.Cooldown);
        return true;
    }

    public void ApplyDamage(IDamageable target, Vector3 attackOrigin)
    {
        if (target == null || !target.IsAlive)
        {
            return;
        }

        target.GetDamage(Mathf.Max(0f, attackData.Damage), attackOrigin);
    }

    public void ResetCooldown()
    {
        attackData.CooldownRemaining = 0f;
    }
}
