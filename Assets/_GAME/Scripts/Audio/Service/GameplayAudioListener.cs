using System;
using UnityEngine;
using Zenject;

// Turns the flow signals the game already fires into sounds. The only class that
// knows both a gameplay event and an SfxId, so adding a sound is an edit here
// plus a catalog row - never a change inside the system that fired the event.
//
// Combat sounds (shot, hit, kill) are not here: they come from the per-unit
// signals in the combat feedback pass and have their own listener.
public class GameplayAudioListener : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IAudioService audio;
    private readonly IPickupService pickupService;
    private readonly AudioCatalog catalog;

    private int coinStreak;
    private float lastCoinTime = float.NegativeInfinity;

    public GameplayAudioListener(
        SignalBus signalBus,
        IAudioService audio,
        IPickupService pickupService,
        AudioCatalog catalog)
    {
        this.signalBus = signalBus;
        this.audio = audio;
        this.pickupService = pickupService;
        this.catalog = catalog;
    }

    public void Initialize()
    {
        signalBus.Subscribe<StartButtleSignal>(OnBattleStarted);
        signalBus.Subscribe<SquadReachedEnemySignal>(OnSquadClashed);
        signalBus.Subscribe<HeroDamagedSignal>(OnHeroDamaged);
        signalBus.Subscribe<HeroDefeatedSignal>(OnHeroDefeated);
        signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        signalBus.Subscribe<LevelCaptureCompletedSignal>(OnCaptureCompleted);

        pickupService.Collected += OnPickupCollected;
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<StartButtleSignal>(OnBattleStarted);
        signalBus.Unsubscribe<SquadReachedEnemySignal>(OnSquadClashed);
        signalBus.Unsubscribe<HeroDamagedSignal>(OnHeroDamaged);
        signalBus.Unsubscribe<HeroDefeatedSignal>(OnHeroDefeated);
        signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        signalBus.Unsubscribe<LevelCaptureCompletedSignal>(OnCaptureCompleted);

        pickupService.Collected -= OnPickupCollected;
    }

    private void OnBattleStarted() => audio.Play(SfxId.BattleStart);
    private void OnSquadClashed() => audio.Play(SfxId.SquadClash);
    private void OnHeroDamaged(HeroDamagedSignal signal) => audio.Play(SfxId.HeroHurt);
    private void OnHeroDefeated() => audio.Play(SfxId.HeroDown);
    private void OnLevelCompleted() => audio.Play(SfxId.LevelWin);
    private void OnCaptureCompleted() => audio.Play(SfxId.CaptureComplete);

    // Rising pitch across an unbroken run of pickups. Same trick the barracks
    // toss already uses, applied to the thing the player does most often.
    private void OnPickupCollected(PickupCollectedEvent collected)
    {
        float now = Time.unscaledTime;
        coinStreak = now - lastCoinTime > catalog.CoinStreakResetSeconds ? 0 : coinStreak + 1;
        lastCoinTime = now;

        float pitch = Mathf.Min(
            catalog.CoinStreakMaxPitch,
            1f + catalog.CoinStreakPitchStep * coinStreak);

        audio.Play(SfxId.CoinPickup, pitch);
    }
}
