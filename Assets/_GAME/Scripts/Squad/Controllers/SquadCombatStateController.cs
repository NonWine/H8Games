using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

// Owns the encounter flow only: which group the formation marches at, and when
// the squad advances, regroups or resets. It no longer hands out combat targets -
// every unit senses for itself through IEnemyCandidateSource.
public class SquadCombatStateController : IInitializable, ITickable, IDisposable, ICombatStateProvider
{
    private const int DefeatResetDelayMilliseconds = 2000;

    private readonly LevelManager levelManager;
    private readonly SignalBus signalBus;
    private readonly EnemyGroupDetector enemyGroupDetector;
    private readonly SquadMovementFacade squadMovementFacade;
    private readonly EnemyDestinationContex  enemyDestinationContex;
    private readonly SquadFormationFacade squadFormationFacade;
    private readonly IPickupService pickupService;

    public EnemyGroupViewController CurrentTargetGroup { get; private set; }
    public CombatFlowState State { get; private set; } = CombatFlowState.IdleInPreparation;

    public SquadCombatStateController(
        SquadMovementFacade squadMovementFacade,
        LevelManager levelManager,
        SignalBus signalBus,
        EnemyGroupDetector enemyGroupDetector,
        EnemyDestinationContex enemyDestinationContex,
        SquadFormationFacade squadFormationFacade,
        IPickupService pickupService)
    {
        this.squadFormationFacade = squadFormationFacade;
        this.squadMovementFacade = squadMovementFacade;
        this.enemyDestinationContex = enemyDestinationContex;
        this.squadMovementFacade = squadMovementFacade;
        this.levelManager = levelManager;
        this.signalBus = signalBus;
        this.enemyGroupDetector = enemyGroupDetector;
        this.pickupService = pickupService;
    }

    private void HandleCombatStartedBattle()
    {
        if (State != CombatFlowState.MovingToZone || CurrentTargetGroup == null)
            return;

        squadMovementFacade.Stop();
        CurrentTargetGroup.Activate();
        State = CombatFlowState.FightingZone;
    }

    // The Cleared event is the fast path out of an encounter, not the only one.
    // Any unit can empty a group now - the hero on his own, a soldier who drifted
    // into range, a level reset - so the flow also re-checks the group it is
    // walking at. A group that was authored empty, or one whose Cleared event
    // never arrives, can no longer strand the squad mid-march.
    public void Tick()
    {
        if (State != CombatFlowState.MovingToZone && State != CombatFlowState.FightingZone)
            return;

        if (IsCurrentGroupEngageable())
            return;

        AdvanceToNextEncounter();
    }

    private bool IsCurrentGroupEngageable()
    {
        return CurrentTargetGroup != null
               && CurrentTargetGroup.State != EnemyGroupState.Cleared
               && CurrentTargetGroup.HasAliveMembers;
    }

    private void HandleCombatClearedZone(EnemyGroupViewController enemyGroup)
    {
        AdvanceToNextEncounter();
    }

    private void AdvanceToNextEncounter()
    {
        if (!TryTargetNearestGroup())
        {
            StartRegroup();
            return;
        }

        State = CombatFlowState.MovingToZone;
        squadMovementFacade.MoveToEnemy();
    }

    public void StartFlow()
    {
        if (State != CombatFlowState.IdleInPreparation || !squadFormationFacade.HasAlly)
            return;

        if (!TryTargetNearestGroup())
            return;

        signalBus.Fire<StartButtleSignal>();
        State = CombatFlowState.MovingToZone;
        squadMovementFacade.MoveToEnemy();
    }

    // Player-initiated alternative to StartFlow: instead of resuming the
    // encounter with whatever progress survived the wipe, hand back every enemy
    // the level started with. Only reachable from the same idle window the
    // defeat recovery already leaves the squad in, same guard as StartFlow.
    public void RestartLevel()
    {
        if (State != CombatFlowState.IdleInPreparation)
            return;

        levelManager.CurrentLevel.ResetRuntimeState();
        pickupService.Clear();
        signalBus.Fire<LevelRestartedSignal>();
    }

    // Subscribe to Cleared the moment the group becomes the target, not when the
    // squad physically reaches it. Ranged soldiers start firing as soon as
    // CurrentTargetGroup is set, so a group can be wiped during the march; if we
    // only subscribed on arrival, that Cleared event would be lost and the squad
    // would get stuck on an already-dead group instead of advancing.
    private bool TryTargetNearestGroup()
    {
        UnsubscribeCurrentGroup();

        CurrentTargetGroup = enemyGroupDetector.FindNearestValidGroup(levelManager.CurrentLevel);
        if (CurrentTargetGroup == null)
            return false;

        CurrentTargetGroup.Cleared += HandleCombatClearedZone;
        enemyDestinationContex.Set(CurrentTargetGroup.transform.position);
        return true;
    }

    private void UnsubscribeCurrentGroup()
    {
        if (CurrentTargetGroup != null)
            CurrentTargetGroup.Cleared -= HandleCombatClearedZone;
    }
    
    // Both the hero dying and the last soldier dying route here, and they can land
    // within the same second, so the reset is guarded: a second run would restart
    // the delay and hand the player a squad that respawns twice.
    //
    // Defeat does not touch enemy or level state at all - only the squad resets.
    // Whatever the player already cleared stays cleared, and the group that wiped
    // them keeps whatever members it has left, so the retry resumes the same
    // encounter instead of handing back a level that looks freshly loaded.
    private async void SetDefeated()
    {
        if (State == CombatFlowState.Defeated)
            return;

        State = CombatFlowState.Defeated;

        ClearCurrentEncounter();

        // Park the root *before* ClearSoldiers teleports it home. The movement
        // state machine kept running through the whole defeat, so the root crawled
        // back out to the enemy group during the delay and the formation slots went
        // with it - soldiers then spawned at the barracks, marched straight into the
        // group that had just wiped the squad, died, and refilled the free slots on
        // a loop.
        squadMovementFacade.EnterPreparationIdle();
        squadFormationFacade.ClearSoldiers();

        await UniTask.Delay(DefeatResetDelayMilliseconds);

        State = CombatFlowState.IdleInPreparation;
        signalBus.Fire<GameIdleStateSignal>();
    }

    // Drops the group reference and its Cleared subscription only. The group's own
    // runtime state (who is still alive, whether it is already Cleared) is left
    // exactly as combat left it - TryTargetNearestGroup will pick the same group
    // back up on the next StartFlow as long as it still has someone standing.
    private void ClearCurrentEncounter()
    {
        enemyDestinationContex.Clear();

        if (CurrentTargetGroup == null)
            return;

        UnsubscribeCurrentGroup();
        CurrentTargetGroup = null;
    }

    // No enemy anywhere in the level is still standing the moment this runs - that
    // is the win condition. Only the signal fires here; the actual level swap
    // (enemies, campaign index) waits for the player to capture CaptureZoneController's
    // zone, since spawning it right now would happen in the player's own face -
    // this is exactly where they are standing.
    private void StartRegroup()
    {
        CurrentTargetGroup = null;
        State = CombatFlowState.Regrouping;
        squadMovementFacade.ReturnHome();

        signalBus.Fire<LevelCompletedSignal>();
    }

    // Squad is physically home: reopen the barracks gate and let whoever
    // survived settle back into formation. Nothing is forced here - the same
    // squad that finished the last level carries into the next one.
    private void HandleSquadRegroupCompleted()
    {
        squadMovementFacade.EnterPreparationIdle();
        squadFormationFacade.RebuildFormation();
        State = CombatFlowState.IdleInPreparation;
    }

    public void Initialize()
    {
        signalBus.Subscribe<SquadReachedEnemySignal>(HandleCombatStartedBattle);
        signalBus.Subscribe<SquadRegroupCompletedSignal>(HandleSquadRegroupCompleted);
        signalBus.Subscribe<SquadDefeatedSignal>(SetDefeated);
        signalBus.Subscribe<HeroDefeatedSignal>(SetDefeated);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<SquadRegroupCompletedSignal>(HandleSquadRegroupCompleted);
        signalBus.Unsubscribe<SquadReachedEnemySignal>(HandleCombatStartedBattle);
        signalBus.Unsubscribe<SquadDefeatedSignal>(SetDefeated);
        signalBus.Unsubscribe<HeroDefeatedSignal>(SetDefeated);
    }
    
}
