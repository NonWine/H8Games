using System;
using System.Globalization;
using UnityEngine;
using Zenject;

public class CombatFloatingTextListener : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IPickupService pickupService;
    private readonly IFloatingTextService floatingText;
    private readonly FloatingTextConfig config;

    public CombatFloatingTextListener(
        SignalBus signalBus,
        IPickupService pickupService,
        IFloatingTextService floatingText,
        FloatingTextConfig config)
    {
        this.signalBus = signalBus;
        this.pickupService = pickupService;
        this.floatingText = floatingText;
        this.config = config;
    }

    public void Initialize()
    {
        signalBus.Subscribe<UnitDamagedSignal>(OnUnitDamaged);
        pickupService.Collected += OnPickupCollected;
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<UnitDamagedSignal>(OnUnitDamaged);
        pickupService.Collected -= OnPickupCollected;
    }

    private void OnUnitDamaged(UnitDamagedSignal signal)
    {
        if (signal.Damage < config.MinDamageToShow)
        {
            return;
        }

        Color color = signal.Side == CombatSide.Enemy
            ? config.EnemyDamageColor
            : config.AllyDamageColor;

        string text = Mathf.RoundToInt(signal.Damage).ToString(CultureInfo.InvariantCulture);
        floatingText.Show(text, color, signal.WorldPosition);
    }

    private void OnPickupCollected(PickupCollectedEvent collected)
    {
        floatingText.Show($"+{collected.Amount}", config.CoinColor, collected.WorldPosition);
    }
}
