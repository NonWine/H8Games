using System;
using UnityEngine;
using Zenject;

public class TerritoryService : IInitializable, ITickable, IDisposable
{
    private readonly ITerritoryView          view;
    private readonly TerritoryConfig         config;
    private readonly TerritoryUnitTracker    tracker;
    private readonly TerritoryDangerCameraFX cameraFX;
    private readonly SignalBus               signalBus;

    private float scanTimer;
    private bool  combatAlertActive;
    private bool  isAnimating;

    [Inject]
    public TerritoryService(
        ITerritoryView view,
        TerritoryConfig config,
        LevelManager levelManager,
        TerritoryDangerCameraFX cameraFX,
        SignalBus signalBus)
    {
        this.view      = view;
        this.config    = config;
        this.cameraFX  = cameraFX;
        this.signalBus = signalBus;
        tracker        = new TerritoryUnitTracker(config, levelManager);
    }

    [Inject]
    private void InjectFlagAnchor(
        [InjectOptional(Id = "TerritoryFlagAnchor")] Transform flagAnchor)
    {
        tracker.SetFlagAnchor(flagAnchor);
    }

    public void Initialize()
    {
        scanTimer = config.UpdateInterval;

        signalBus.Subscribe<StartButtleSignal>(OnBattleStart);
        signalBus.Subscribe<GameIdleStateSignal>(OnGameIdle);
        signalBus.Subscribe<LoadNextLevelSignal>(OnLoadNextLevel);
    }

    public void Tick()
    {
        float dt = Time.deltaTime;

        scanTimer += dt;
        bool scanChanged = false;

        if (scanTimer >= config.UpdateInterval)
        {
            scanTimer   = 0f;
            scanChanged = tracker.Scan(out bool levelChanged);

            if (levelChanged)
            {
                view.Clear();
                ResetCombatAlert();
                return;
            }
        }

        tracker.UpdatePositions(dt);

        // Always call Refresh every tick so boundary smoothing animates continuously.
        view.Refresh(tracker.SmoothedPositions, config, dt);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<StartButtleSignal>(OnBattleStart);
        signalBus.Unsubscribe<GameIdleStateSignal>(OnGameIdle);
        signalBus.Unsubscribe<LoadNextLevelSignal>(OnLoadNextLevel);

        view.Clear();
        ResetCombatAlert();
        tracker.Reset();
    }

    private void OnBattleStart()
    {
        isAnimating = true;
        view.SetAnimating(true);

        combatAlertActive = true;
        view.SetCombatAlert(true);
        cameraFX.SetDangerState(true);
    }

    private void OnGameIdle()
    {
        isAnimating = false;
        view.SetAnimating(false);
        ResetCombatAlert();
    }

    private void OnLoadNextLevel()
    {
        isAnimating = false;
        view.SetAnimating(false);
        ResetCombatAlert();
    }

    private void ResetCombatAlert()
    {
        if (!combatAlertActive)
            return;

        combatAlertActive = false;
        view.SetCombatAlert(false);
        cameraFX.SetDangerState(false);
    }
}
