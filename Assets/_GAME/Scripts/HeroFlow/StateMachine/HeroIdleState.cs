public class HeroIdleState : HeroStateBase
{
    private readonly HeroAutoAttackLogger logger;

    public HeroIdleState(
        HeroRuntimeModel model,
        HeroAnimationController heroAnimationController,
        HeroAutoAttackLogger logger)
        : base(model, heroAnimationController)
    {
        this.logger = logger;
    }

    public override void Enter()
    {
        heroAnimationController.PlayIdle();
    }

    public override void Tick()
    {
        if (Hero.HasValidTarget)
        {
            logger.LogTargetAcquired(Hero.CurrentTarget);
            ChangeState<HeroAttackState>();
        }
    }

    public override void Exit()
    {
    }
}
