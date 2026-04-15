using System;
using Zenject;

public enum CombatFlowState
{
    IdleInPreparation = 0,
    MovingToZone = 1,
    FightingZone = 2,
    Regrouping = 3,
    Defeated = 4
}

public class CombatStateController : IInitializable, IDisposable , IEnemyGroupProvider
{
    private readonly LevelManager levelManager;
    private readonly SignalBus signalBus;
    private readonly EnemyGroupDetector enemyGroupDetector;
    private readonly SquadMovementFacade  squadMovementFacade;
    
    public EnemyGroupFacade CurrentTargetGroup { get; set; }
    public CombatFlowState State { get; private set; } = CombatFlowState.IdleInPreparation;    

    public CombatStateController(
        SquadMovementFacade squadMovementFacade,
        LevelManager levelManager,
        SignalBus signalBus,
        EnemyGroupDetector enemyGroupDetector)
    {
        this.squadMovementFacade = squadMovementFacade;
        this.levelManager = levelManager;
        this.signalBus = signalBus;
        this.enemyGroupDetector = enemyGroupDetector;
    }
    
    public void HandleCombatStartedBattle(EnemyGroupFacade enemyGroup)
    {
        CurrentTargetGroup = enemyGroup;
        squadMovementFacade.Stop();
        State = CombatFlowState.FightingZone;
        
    }

    public void HandleCombatClearedZone(EnemyGroupFacade enemyGroup)
    {
        CurrentTargetGroup = enemyGroupDetector.FindNearestValidGroup(levelManager.CurrentLevel);
        if (CurrentTargetGroup == null)
        {
            StartRegroup();
            return;
        }

        State = CombatFlowState.MovingToZone;
        squadMovementFacade.MoveToEnemy();
    }
    
    public void StartFlow()
    {
        if (State != CombatFlowState.IdleInPreparation)
            return;

        CurrentTargetGroup = enemyGroupDetector.FindNearestValidGroup(levelManager.CurrentLevel);
        
        State = CombatFlowState.MovingToZone;
        squadMovementFacade.MoveToEnemy();
    }
    
    public void SetDefeated()
    {
        CurrentTargetGroup = null;
        State = CombatFlowState.Defeated;
        squadMovementFacade.Stop();
    }
    
    
    private void StartRegroup()
    {
        CurrentTargetGroup = null;
        State = CombatFlowState.Regrouping;
        squadMovementFacade.ReturnHome();
    }
    
    private void HandleSquadRegroupCompleted()
    {
        signalBus.Fire(new LoadNextLevelSignal());
    }

    public void Initialize()
    {
        signalBus.Subscribe<SquadRegroupCompletedSignal>(HandleSquadRegroupCompleted);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<SquadRegroupCompletedSignal>(HandleSquadRegroupCompleted);

    }
}