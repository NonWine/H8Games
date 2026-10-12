using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SoldierMoveState : SoldierStateBase
{
    private readonly ISquadMovementStateReader movementStateReader;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    private readonly SquadFollowSettings squadFollowSettings;
    private readonly ISoldierFormationMover formationMover;
    private readonly SignalBus signalBus;
    private readonly ICombatStateProvider combatStateProvider;
    private readonly IEnemyCandidateSource candidateSource;

    public SoldierMoveState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        ISquadMovementStateReader movementStateReader,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        SquadFollowSettings squadFollowSettings,
        ISoldierFormationMover formationMover,
        SignalBus signalBus,
        [InjectOptional] ICombatStateProvider combatStateProvider = null,
        [InjectOptional] IEnemyCandidateSource candidateSource = null)
        : base(model, modules, agentAnimationController)
    {
        this.movementStateReader = movementStateReader;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.squadFollowSettings = squadFollowSettings;
        this.formationMover = formationMover;
        this.signalBus = signalBus;
        this.combatStateProvider = combatStateProvider;
        this.candidateSource = candidateSource;
    }

    public override void Enter()
    {
        formationMover.Reset();
        agentAnimationController.SetAnimationState(UnitState.Move);
    }

    public override void Tick()
    {
        if (ShouldEngageTarget())
        {
            ChangeState<SoldierAttackState>();
            return;
        }

        bool inBattle = combatStateProvider != null && combatStateProvider.State == CombatFlowState.FightingZone;

        if (inBattle)
        {
            UpdateBattleAssaultMovement();
            return;
        }

        if (!Soldier.HasFormationAssignment)
        {
            ChangeState<SoldierIdleState>();
            return;
        }

        Vector3 slotCenter = Soldier.GetAssignedSlotCenter(squadSlotPositionProvider);

        if (movementStateReader.IsMoving)
        {
            formationMover.MoveToSlot(
                Soldier.SquadRootView.transform,
                slotCenter,
                true,
                Time.deltaTime);
            return;
        }

        if (formationMover.IsAt(slotCenter, squadFollowSettings.SlotReachThreshold))
        {
            signalBus.Fire(new SoldierReachedSlotSignal(Soldier.AssignedSlot.Index));
            ChangeState<SoldierIdleState>();
            return;
        }

        UpdateIdleSlotMovement(slotCenter);
    }

    private void UpdateBattleAssaultMovement()
    {
        float assaultStoppingDistance = squadFollowSettings.EngageRange * 0.85f;

        if (Soldier.HasValidTarget)
        {
            formationMover.MoveTowardsPosition(
                Soldier.CurrentTarget.transform.position,
                assaultStoppingDistance,
                Time.deltaTime);
            return;
        }

        ITargetSelectionCandidate nearestLiving = GetNearestLivingEnemyCandidate();
        if (nearestLiving != null)
        {
            formationMover.MoveTowardsPosition(
                nearestLiving.Position,
                assaultStoppingDistance,
                Time.deltaTime);
            return;
        }

        formationMover.Stop();
        ChangeState<SoldierIdleState>();
    }

    private ITargetSelectionCandidate GetNearestLivingEnemyCandidate()
    {
        if (candidateSource == null)
        {
            return null;
        }

        IReadOnlyList<ITargetSelectionCandidate> candidates = candidateSource.Candidates;
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        Vector3 myPos = Soldier.Transform.position;
        ITargetSelectionCandidate best = null;
        float bestDistSq = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            ITargetSelectionCandidate c = candidates[i];
            if (c == null || !c.IsAlive)
            {
                continue;
            }

            Vector3 delta = c.Position - myPos;
            delta.y = 0f;
            float distSq = delta.sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = c;
            }
        }

        return best;
    }

    public override void Exit()
    {
    }

    // While the squad root is still marching, hold the formation slot unless the
    // enemy is genuinely in range. Engaging on first target acquisition froze
    // soldiers mid-approach wherever they happened to stand, so the formation
    // spacing never applied and their meshes ended up overlapping.
    private bool ShouldEngageTarget()
    {
        if (!Soldier.HasValidTarget)
        {
            return false;
        }

        Vector3 toTarget = Soldier.CurrentTarget.transform.position - Soldier.Transform.position;
        toTarget.y = 0f;

        float engageRange = squadFollowSettings.EngageRange;

        return toTarget.sqrMagnitude <= engageRange * engageRange;
    }

    private void UpdateIdleSlotMovement(Vector3 slotCenter)
    {
        formationMover.MoveToSlot(
            Soldier.SquadRootView.transform,
            slotCenter,
            false,
            Time.deltaTime);
    }
}
