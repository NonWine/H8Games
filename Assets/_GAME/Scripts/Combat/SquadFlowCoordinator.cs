using UnityEngine;
using Zenject;

public sealed class SquadFlowCoordinator : MonoBehaviour
{
    private readonly System.Collections.Generic.List<EnemyGroupFacade> candidateGroups = new();

    private SquadRoot squadRoot;
    private SquadCombatCoordinator squadCombatCoordinator;
    private LevelManager levelManager;
    private GamePhaseService gamePhaseService;
    private EnemyGroupFacade currentTargetGroup;
    private bool waitingForRegroupFormation;
    private bool waitingForLevelTransition;

    public EnemyGroupFacade CurrentTargetGroup => currentTargetGroup;
    public LevelRuntime CurrentLevel => levelManager?.CurrentLevel;

    [Inject]
    public void Construct(
        SquadRoot squadRoot,
        SquadCombatCoordinator squadCombatCoordinator,
        LevelManager levelManager,
        GamePhaseService gamePhaseService)
    {
        this.squadRoot = squadRoot;
        this.squadCombatCoordinator = squadCombatCoordinator;
        this.levelManager = levelManager;
        this.gamePhaseService = gamePhaseService;
    }

    private void Awake()
    {
        waitingForRegroupFormation = false;
        waitingForLevelTransition = false;
    }

    private void OnEnable()
    {
        squadRoot.MovementTargetReached += HandleMovementTargetReached;
        squadCombatCoordinator.EncounterStarted += HandleEncounterStarted;
        squadCombatCoordinator.EncounterCleared += HandleEncounterCleared;
        squadCombatCoordinator.SquadDefeated += HandleSquadDefeated;

        if (gamePhaseService != null)
            gamePhaseService.Changed += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        squadRoot.MovementTargetReached -= HandleMovementTargetReached;
        squadCombatCoordinator.EncounterStarted -= HandleEncounterStarted;
        squadCombatCoordinator.EncounterCleared -= HandleEncounterCleared;
        squadCombatCoordinator.SquadDefeated -= HandleSquadDefeated;

        if (gamePhaseService != null)
            gamePhaseService.Changed -= HandlePhaseChanged;
    }

    private void Update()
    {
        if (!waitingForRegroupFormation)
            return;

        if (!squadRoot.IsFormationSettled)
            return;

        waitingForRegroupFormation = false;
        if (waitingForLevelTransition)
        {
            waitingForLevelTransition = false;
            levelManager?.TryAdvanceToNextLevelOrRestart();
        }

        gamePhaseService?.EnterPreparation();
        squadRoot.EnterIdleInPreparation();
    }

    public void StartFlow()
    {
        if (gamePhaseService != null && !gamePhaseService.IsPreparation)
            return;

        if (!squadCombatCoordinator.HasLivingAllies)
        {
            SetDefeated();
            return;
        }

        waitingForRegroupFormation = false;
        waitingForLevelTransition = false;
        gamePhaseService?.EnterBattle();
        SearchAndMoveToNextZone();
    }

    public void NotifyEncounterZoneEntered(EnemyGroupFacade enemyGroup)
    {
        if (enemyGroup == null || squadRoot.State != SquadRootState.MovingToZone)
            return;

        if (currentTargetGroup != enemyGroup)
            return;

        TryStartEncounter(enemyGroup);
    }

    private void SearchAndMoveToNextZone()
    {
        squadRoot.StartSearchingForNextZone();
        currentTargetGroup = FindNearestValidGroup();

        if (currentTargetGroup == null)
        {
            ReturnToRegroup();
            return;
        }

        squadRoot.MoveToZone(currentTargetGroup.EngagePointPosition);
    }

    private EnemyGroupFacade FindNearestValidGroup()
    {
        LevelRuntime currentLevel = CurrentLevel;
        if (currentLevel == null)
            return null;

        currentLevel.RebuildGroups();
        candidateGroups.Clear();
        candidateGroups.AddRange(currentLevel.Groups);

        EnemyGroupFacade nearestGroup = null;
        float nearestSqrDistance = float.MaxValue;
        Vector3 squadPosition = squadRoot.transform.position;

        for (int i = 0; i < candidateGroups.Count; i++)
        {
            EnemyGroupFacade group = candidateGroups[i];
            if (group == null || !group.IsAvailableForEncounter)
                continue;

            Vector3 delta = group.EngagePointPosition - squadPosition;
            delta.y = 0f;
            float sqrDistance = delta.sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance)
                continue;

            nearestGroup = group;
            nearestSqrDistance = sqrDistance;
        }

        return nearestGroup;
    }

    private void HandleMovementTargetReached()
    {
        if (squadRoot.State == SquadRootState.MovingToZone && currentTargetGroup != null)
        {
            TryStartEncounter(currentTargetGroup);
            return;
        }

        if (squadRoot.State == SquadRootState.ReturningToRegroup)
            waitingForRegroupFormation = true;
    }

    private void HandleEncounterStarted(EnemyGroupFacade enemyGroup)
    {
        if (enemyGroup == null)
            return;

        currentTargetGroup = enemyGroup;
        squadRoot.StopForEncounter();
    }

    private void HandleEncounterCleared(EnemyGroupFacade enemyGroup)
    {
        if (enemyGroup != currentTargetGroup)
            return;

        currentTargetGroup = null;
        SearchAndMoveToNextZone();
    }

    private void HandleSquadDefeated()
    {
        SetDefeated();
    }

    private void TryStartEncounter(EnemyGroupFacade enemyGroup)
    {
        if (enemyGroup == null)
            return;

        if (!enemyGroup.IsAvailableForEncounter)
        {
            SearchAndMoveToNextZone();
            return;
        }

        squadCombatCoordinator.TryBeginEncounter(enemyGroup);
    }

    private void ReturnToRegroup()
    {
        currentTargetGroup = null;
        waitingForRegroupFormation = false;
        waitingForLevelTransition = true;
        squadRoot.StartReturningToRegroup(squadRoot.HomePosition);
    }

    private void SetDefeated()
    {
        waitingForRegroupFormation = false;
        waitingForLevelTransition = false;
        currentTargetGroup = null;
        squadRoot.MarkDefeated();
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase != GamePhase.Preparation)
            return;

        if (squadRoot.State == SquadRootState.Defeated || waitingForRegroupFormation)
            return;

        if (squadRoot.State != SquadRootState.WaitingStart)
            squadRoot.EnterIdleInPreparation();
    }
}
