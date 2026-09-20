using UnityEngine;

public class HeroAttackState : HeroStateBase
{
    private readonly BaseCombatUnitView combatView;
    private readonly IAttackModule attackModule;
    private readonly AttackRuntimeModel attackData;
    private readonly UnitStats unitStats;
    private readonly HeroAutoAttackLogger logger;

    public HeroAttackState(
        HeroRuntimeModel model,
        HeroAnimationController heroAnimationController,
        BaseCombatUnitView combatView,
        IAttackModule attackModule,
        AttackRuntimeModel attackData,
        UnitStats unitStats,
        HeroAutoAttackLogger logger)
        : base(model, heroAnimationController)
    {
        this.combatView = combatView;
        this.attackModule = attackModule;
        this.attackData = attackData;
        this.unitStats = unitStats;
        this.logger = logger;
    }

    public override void Enter()
    {
        attackData.CooldownRemaining = 0f;
        heroAnimationController.PlayAttack();
    }

    public override void Tick()
    {
        if (!Hero.HasValidTarget)
        {
            logger.LogTargetLost();
            ChangeState<HeroIdleState>();
            return;
        }

        attackData.CooldownRemaining -= Time.deltaTime;

        if (attackData.CooldownRemaining <= 0f)
        {
            HandleAttack();
            attackData.CooldownRemaining = attackData.GetRandomizedCooldown();
            logger.LogShotFired(Hero.CurrentTarget, attackData.CooldownRemaining);
        }
    }

    public override void Exit()
    {
        attackData.CooldownRemaining = attackData.GetRandomizedCooldown();
    }

    private void HandleAttack()
    {
        if (!Hero.IsAlive || !Hero.HasValidTarget)
        {
            return;
        }

        ICombatTarget target = Hero.CurrentTarget;
        attackModule.HandleAttack(
            target,
            combatView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, combatView.AttackPoint.position));
    }

}
