using System;
using Zenject;

// Reacts to HeroDefeatedSignal from outside the hero's own construction graph on
// purpose: IPickupService's carry sink resolves IPickupCarryAnchorProvider, which
// resolves PlayerView and spawns the hero, so anything the hero itself builds
// (HeroDeadState included) must not depend back on IPickupService - that closes
// a cycle back onto PickupService while it is still constructing itself.
public class PickupDefeatBridge : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IPickupService pickupService;

    public PickupDefeatBridge(SignalBus signalBus, IPickupService pickupService)
    {
        this.signalBus = signalBus;
        this.pickupService = pickupService;
    }

    public void Initialize()
    {
        signalBus.Subscribe<HeroDefeatedSignal>(HandleHeroDefeated);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<HeroDefeatedSignal>(HandleHeroDefeated);
    }

    private void HandleHeroDefeated()
    {
        pickupService.DiscardCarried();
    }
}
