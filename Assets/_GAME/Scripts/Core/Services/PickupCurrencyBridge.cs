using System;
using Zenject;

public sealed class PickupCurrencyBridge : IInitializable, IDisposable
{
    private readonly IPickupService pickupService;
    private readonly CurrencyService currencyService;

    public PickupCurrencyBridge(IPickupService pickupService, CurrencyService currencyService)
    {
        this.pickupService   = pickupService;
        this.currencyService = currencyService;
    }

    public void Initialize()
    {
        pickupService.Collected += OnCollected;
    }

    public void Dispose()
    {
        pickupService.Collected -= OnCollected;
    }

    private void OnCollected(PickupCollectedEvent e)
    {
        currencyService.Add(e.Amount);
    }
}
