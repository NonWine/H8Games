using Zenject;

public class HeroDeadState : HeroStateBase
{
    private readonly SignalBus signalBus;

    public HeroDeadState(
        HeroRuntimeModel model,
        HeroAnimationController heroAnimationController,
        SignalBus signalBus)
        : base(model, heroAnimationController)
    {
        this.signalBus = signalBus;
    }

    public override void Enter()
    {
        heroAnimationController.PlayDead();
        signalBus.Fire<HeroDefeatedSignal>();
    }

    public override void Exit()
    {
    }
}
