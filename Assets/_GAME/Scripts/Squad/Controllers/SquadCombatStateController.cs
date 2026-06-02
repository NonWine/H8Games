using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class SquadCombatStateController : IInitializable, IDisposable, IEnemyGroupProvider ,ICombatStateProvider
{
    private readonly LevelManager levelManager;
    private readonly SignalBus signalBus;
    private readonly EnemyGroupDetector enemyGroupDetector;
    private readonly SquadMovementFacade squadMovementFacade;
    private readonly EnemyDestinationContex  enemyDestinationContex;
    private readonly SquadFormationFacade squadFormationFacade;
    
    public EnemyGroupViewController CurrentTargetGroup { get; set; }
    public CombatFlowState State { get; private set; } = CombatFlowState.IdleInPreparation;

    public SquadCombatStateController(
        SquadMovementFacade squadMovementFacade,
        LevelManager levelManager,
        SignalBus signalBus,
        EnemyGroupDetector enemyGroupDetector,
        EnemyDestinationContex enemyDestinationContex,
        SquadFormationFacade squadFormationFacade)
    {
        this.squadFormationFacade = squadFormationFacade;
        this.squadMovementFacade = squadMovementFacade;
        this.enemyDestinationContex = enemyDestinationContex;
        this.squadMovementFacade = squadMovementFacade;
        this.levelManager = levelManager;
        this.signalBus = signalBus;
        this.enemyGroupDetector = enemyGroupDetector;
    }

    private void HandleCombatStartedBattle()
    {
        if (State != CombatFlowState.MovingToZone || CurrentTargetGroup == null)
            return;

        squadMovementFacade.Stop();
        CurrentTargetGroup.Activate();
        State = CombatFlowState.FightingZone;
    }

    private void HandleCombatClearedZone(EnemyGroupViewController enemyGroup)
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
    
    private async void SetDefeated()
    {
        ClearCurrentEncounter();
        squadFormationFacade.ClearSoldiers();
        State = CombatFlowState.Defeated;
        await UniTask.Delay(2000);
        levelManager.CurrentLevel.ResetRuntimeState();
        State = CombatFlowState.IdleInPreparation;
        signalBus.Fire<GameIdleStateSignal>();
    }

    private void ClearCurrentEncounter()
    {
        if (CurrentTargetGroup == null)
            return;

        UnsubscribeCurrentGroup();
        CurrentTargetGroup.ResetRuntimeState();
        CurrentTargetGroup = null;
    }

    private void StartRegroup()
    {
        CurrentTargetGroup = null;
        State = CombatFlowState.Regrouping;
        squadMovementFacade.ReturnHome();
    }

    private void HandleSquadRegroupCompleted()
    {
        squadMovementFacade.EnterPreparationIdle();
        squadFormationFacade.RebuildFormation();
        signalBus.Fire(new LoadNextLevelSignal());
        State = CombatFlowState.IdleInPreparation;
    }

    public void Initialize()
    {
        signalBus.Subscribe<SquadReachedEnemySignal>(HandleCombatStartedBattle);
        signalBus.Subscribe<SquadRegroupCompletedSignal>(HandleSquadRegroupCompleted);
        signalBus.Subscribe<SquadDefeatedSignal>(SetDefeated);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<SquadRegroupCompletedSignal>(HandleSquadRegroupCompleted);
        signalBus.Unsubscribe<SquadReachedEnemySignal>(HandleCombatStartedBattle);
        signalBus.Unsubscribe<SquadDefeatedSignal>(SetDefeated);
    }
    
}
