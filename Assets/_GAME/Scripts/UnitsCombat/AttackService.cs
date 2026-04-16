using UnityEngine;

public class UnitAttackAgentHandler
{
    private readonly AttackRuntimeModel attackData;

    public UnitAttackAgentHandler(AttackRuntimeModel attackData)
    {
        this.attackData = attackData;
    }

    public bool Tick(float deltaTime, IDamageable target, Vector3 attackOrigin)
    {
        attackData.CooldownRemaining = Mathf.Max(0f, attackData.CooldownRemaining - deltaTime);

        if (target == null || !target.IsAlive || attackData.CooldownRemaining > 0f)
        {
            return false;
        }

        attackData.CooldownRemaining = Mathf.Max(0.05f, attackData.Cooldown);
        target.GetDamage(Mathf.Max(0f, attackData.Damage), attackOrigin);
        return true;
    }

    public void ResetCooldown()
    {
        attackData.CooldownRemaining = 0f;
    }
}