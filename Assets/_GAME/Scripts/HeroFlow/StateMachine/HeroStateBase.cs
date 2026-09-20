public abstract class HeroStateBase : State<HeroStateBase>
{
    protected readonly HeroRuntimeModel model;
    protected readonly HeroAnimationController heroAnimationController;

    protected HeroStateBase(HeroRuntimeModel model, HeroAnimationController heroAnimationController)
    {
        this.model = model;
        this.heroAnimationController = heroAnimationController;
    }

    protected HeroRuntimeModel Hero => model;
}
