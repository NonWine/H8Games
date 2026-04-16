using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(SoldierFollower))]
public class SoldierCombatAgent : BaseTargetingCombatAgent
{
    [field: SerializeField] public SoldierFollower SoldierFollower { get; private set; }

    private IEnemyGroupProvider currentEnemyGroupProvider;
    private ISquadMovementStateReader stateReader;

    public event Action<SoldierCombatAgent> OnDiedEvent; 
    
    [Inject]
    public void Construct(IEnemyGroupProvider currentEnemyGroupProvider, ISquadMovementStateReader stateReader)
    {
        this.currentEnemyGroupProvider = currentEnemyGroupProvider;
        this.stateReader = stateReader;
    }
    

    private void OnDisable()
    {
        modules.TargetTracker.SetCurrentTarget(null, this);
        modules.Reservation.ClearReservations();
    }

    protected override void Update()
    {
        base.Update();
        if (!IsAlive)
            return;
        EnemyGroupViewController currentGroup = currentEnemyGroupProvider.CurrentTargetGroup;
        var tracker = modules.TargetTracker;

        if (currentGroup == null || currentGroup.State != EnemyGroupState.Activated)
        {
            tracker.SetCurrentTarget(null, this);
            State = UnitState.Idle;
            return;
        }

        if (!tracker.IsCurrentTargetValid(currentGroup))
        {
            TryAcquireTarget(currentGroup);
        }
        else if (tracker.ShouldRetarget())
        {
            TryAcquireTarget(currentGroup);
        }

        if (!tracker.IsCurrentTargetValid(currentGroup))
        {
            tracker.SetCurrentTarget(null, this);
            State = UnitState.Idle;
            return;
        }

        tracker.RotateTowardsCurrentTarget(transform);

        State = UnitState.Attack;

        if (modules.Attack.Tick(Time.deltaTime, tracker.CurrentTarget, CombatView.AttackPoint.position))
        {
            modules.ProjectileSpawner.Spawn(
                CombatView.AttackPoint,
                tracker.CurrentTarget.transform,
                stats.ProjectileSpeed);
        }
    }

    private void UpdateFormation()
    {
        if(State == UnitState.Attack || State == UnitState.Dead) return;
    
        SoldierFollower.UpdateFormation();
        if (!stateReader.IsMoving)
        {
            if (SoldierFollower.State == SoldierFormationState.WaitingInFormation)
            {
                State = UnitState.Idle;
            }
            else if (SoldierFollower.State == SoldierFormationState.MovingToSlot)
            {
                State = UnitState.Move;
            }
            
        }
        else
        {
            State = UnitState.Move;
        }
    }

    private void LateUpdate()
    {
        if (!IsAlive)
            return;
        
        UpdateFormation();
    }

    private void TryAcquireTarget(EnemyGroupViewController currentGroup)
    {
        var tracker = modules.TargetTracker;

        if (currentGroup == null)
        {
            tracker.SetCurrentTarget(null, this);
            return;
        }

        ICombatTarget target = currentGroup.GetBestLivingEnemyTarget(
            transform.position,
            stats.ReservationPenalty);

        if (target == null)
        {
            tracker.SetCurrentTarget(null, this);
            tracker.ResetTargetingTimers();
            return;
        }

        tracker.SetCurrentTarget(target, this);
        tracker.MarkRetargetWindow();
    }

    protected override void OnDied()
    {
        OnDiedEvent?.Invoke(this);
        base.OnDied();
    }
}