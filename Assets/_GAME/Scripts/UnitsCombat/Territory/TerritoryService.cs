using System;
using UnityEngine;
using Zenject;

public class TerritoryService : IInitializable, ITickable, IDisposable, ITerritoryCaptureFocusProvider
{
    private readonly ITerritoryView          view;
    private readonly TerritoryConfig         config;
    private readonly TerritoryUnitTracker    tracker;
    private readonly TerritoryDangerCameraFX cameraFX;
    private readonly SignalBus               signalBus;

    private float scanTimer;
    private bool  combatEffectsActive;

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

    public void Initialize()
    {
        scanTimer = config.UpdateInterval;

        signalBus.Subscribe<StartButtleSignal>(OnBattleStart);
        signalBus.Subscribe<GameIdleStateSignal>(OnBattleEnded);

        // Every way a battle can end, not just the idle state that trails a
        // defeat by two seconds: the pulse, the border sparks and the danger
        // post FX used to keep running through the whole capture phase.
        signalBus.Subscribe<SquadDefeatedSignal>(OnBattleEnded);
        signalBus.Subscribe<HeroDefeatedSignal>(OnBattleEnded);
        signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);

        signalBus.Subscribe<LoadNextLevelSignal>(OnLevelSwapped);
        signalBus.Subscribe<LevelRestartedSignal>(OnLevelSwapped);
    }

    public void Tick()
    {
        float dt = Time.deltaTime;

        scanTimer += dt;

        if (scanTimer >= config.UpdateInterval)
        {
            scanTimer = 0f;
            tracker.Scan(out bool levelChanged);

            if (levelChanged)
            {
                view.Clear();
                SetCombatEffects(false);
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
        signalBus.Unsubscribe<GameIdleStateSignal>(OnBattleEnded);
        signalBus.Unsubscribe<SquadDefeatedSignal>(OnBattleEnded);
        signalBus.Unsubscribe<HeroDefeatedSignal>(OnBattleEnded);
        signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        signalBus.Unsubscribe<LoadNextLevelSignal>(OnLevelSwapped);
        signalBus.Unsubscribe<LevelRestartedSignal>(OnLevelSwapped);

        view.Clear();
        SetCombatEffects(false);
        tracker.Reset();
    }

    // ── ITerritoryCaptureFocusProvider ────────────────────────────────────────

    public bool TryGetCaptureFocus(out Vector3 worldPosition)
    {
        return tracker.TryGetCaptureFocus(out worldPosition);
    }

    // ── signal handlers ───────────────────────────────────────────────────────

    private void OnBattleStart()
    {
        tracker.ExitCaptureFocus();
        SetCombatEffects(true);
    }

    private void OnBattleEnded()
    {
        SetCombatEffects(false);
    }

    // The last enemy is down. Instead of letting the zone collapse, freeze it on
    // the spot they fell: the existing shrink smoothing then walks the boundary
    // that is already on screen down into a MinRadius circle there, so the capture
    // ring grows out of the territory rather than popping in from nothing.
    private void OnLevelCompleted()
    {
        SetCombatEffects(false);
        tracker.EnterCaptureFocus();
    }

    // Dropping the tracked units here rather than waiting for the next scan matters:
    // for up to one UpdateInterval they still hold the outgoing level's positions,
    // which is long enough to flash the old shape over the new level.
    private void OnLevelSwapped()
    {
        SetCombatEffects(false);
        tracker.Reset();
        view.Clear();
        scanTimer = config.UpdateInterval;
    }

    private void SetCombatEffects(bool active)
    {
        if (combatEffectsActive == active)
            return;

        combatEffectsActive = active;

        view.SetAnimating(active);
        view.SetCombatAlert(active);
        cameraFX.SetDangerState(active);
    }
}
