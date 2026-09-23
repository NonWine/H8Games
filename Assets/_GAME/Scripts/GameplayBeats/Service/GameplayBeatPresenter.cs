using System;
using Zenject;

public class GameplayBeatPresenter : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly GameplayBeatView view;
    private readonly GameplayBeatConfig config;

    public GameplayBeatPresenter(
        SignalBus signalBus,
        GameplayBeatView view,
        GameplayBeatConfig config)
    {
        this.signalBus = signalBus;
        this.view = view;
        this.config = config;
    }

    public void Initialize()
    {
        signalBus.Subscribe<StartButtleSignal>(ShowBattleStarted);
        signalBus.Subscribe<SquadReachedEnemySignal>(ShowEncounterStarted);
        signalBus.Subscribe<LevelCompletedSignal>(ShowLevelCleared);
        signalBus.Subscribe<LevelCaptureCompletedSignal>(ShowLevelCaptured);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<StartButtleSignal>(ShowBattleStarted);
        signalBus.Unsubscribe<SquadReachedEnemySignal>(ShowEncounterStarted);
        signalBus.Unsubscribe<LevelCompletedSignal>(ShowLevelCleared);
        signalBus.Unsubscribe<LevelCaptureCompletedSignal>(ShowLevelCaptured);
    }

    private void ShowBattleStarted()
    {
        view.Show(
            config.BattleStartedText,
            config.BattleStartedColor,
            config.BattleStartedHoldDuration,
            config);
    }

    private void ShowEncounterStarted()
    {
        view.Show(
            config.EncounterStartedText,
            config.EncounterStartedColor,
            config.EncounterStartedHoldDuration,
            config);
    }

    private void ShowLevelCleared()
    {
        view.Show(
            config.LevelClearedText,
            config.LevelClearedColor,
            config.LevelClearedHoldDuration,
            config);
    }

    private void ShowLevelCaptured()
    {
        view.Show(
            config.LevelCapturedText,
            config.LevelCapturedColor,
            config.LevelCapturedHoldDuration,
            config);
    }
}
