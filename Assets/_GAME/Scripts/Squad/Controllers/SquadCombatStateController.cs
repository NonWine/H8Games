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

        CurrentTargetGroup.Cleared += HandleCombatClearedZone;
        squadMovementFacade.Stop();
        CurrentTargetGroup.Activate();
        State = CombatFlowState.FightingZone;
    }

    private void HandleCombatClearedZone(EnemyGroupViewController enemyGroup)
    {
        if(CurrentTargetGroup != null) CurrentTargetGroup.Cleared -= HandleCombatClearedZone;
      
        CurrentTargetGroup = null;
        CurrentTargetGroup = enemyGroupDetector.FindNearestValidGroup(levelManager.CurrentLevel);
        if (CurrentTargetGroup == null)
        {
            StartRegroup();
            return;
        }

        State = CombatFlowState.MovingToZone;
        enemyDestinationContex.Set(CurrentTargetGroup.transform.position);
        squadMovementFacade.MoveToEnemy();
    }

    public void StartFlow()
    {
        if (State != CombatFlowState.IdleInPreparation || !squadFormationFacade.HasAlly)
            return;

        CurrentTargetGroup = enemyGroupDetector.FindNearestValidGroup(levelManager.CurrentLevel);
        if (CurrentTargetGroup == null)
            return;
        signalBus.Fire<StartButtleSignal>();
        enemyDestinationContex.Set(CurrentTargetGroup.transform.position);
        State = CombatFlowState.MovingToZone;
        squadMovementFacade.MoveToEnemy();
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

        CurrentTargetGroup.Cleared -= HandleCombatClearedZone;
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
