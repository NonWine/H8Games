using System;
using UnityEngine;
using Zenject;

public class FloatingNumberPresenter : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IPickupService pickupService;
    private readonly FloatingNumberPool pool;
    private readonly FloatingNumberConfig config;

    public FloatingNumberPresenter(
        SignalBus signalBus,
        IPickupService pickupService,
        FloatingNumberPool pool,
        FloatingNumberConfig config)
    {
        this.signalBus = signalBus;
        this.pickupService = pickupService;
        this.pool = pool;
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
        if (signal.Damage <= 0f)
        {
            return;
        }

        Color color = signal.Side == CombatSide.Enemy
            ? config.EnemyDamageColor
            : config.FriendlyDamageColor;
        float scale = signal.WasLethal ? config.LethalScale : 1f;

        Spawn(
            Mathf.CeilToInt(signal.Damage).ToString(),
            signal.WorldPosition + config.DamageOffset,
            color,
            config.DamageFontSize,
            scale);
    }

    private void OnPickupCollected(PickupCollectedEvent collected)
    {
        if (collected.Amount <= 0)
        {
            return;
        }

        Spawn(
            $"+{collected.Amount}",
            collected.WorldPosition + config.CoinOffset,
            config.CoinColor,
            config.CoinFontSize,
            1f);
    }

    private void Spawn(string value, Vector3 position, Color color, float fontSize, float scale)
    {
        Vector2 jitter = UnityEngine.Random.insideUnitCircle * config.LateralJitter;
        position += new Vector3(jitter.x, 0f, jitter.y);

        FloatingNumberView view = pool.Spawn();
        view.Show(
            value,
            position,
            color,
            fontSize,
            config.Duration,
            config.RiseDistance,
            scale,
            pool.Despawn);
    }
}
