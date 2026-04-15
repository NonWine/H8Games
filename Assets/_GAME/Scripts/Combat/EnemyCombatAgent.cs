using System;
using UnityEngine;
using Zenject;

public class EnemyCombatAgent : BaseTargetingCombatAgent
{
    [Inject] private IAllyTargetProvider allyTargetProvider;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    public EnemyGroupViewController Group { get; private set; }
    public StaticEnemyState State { get; private set; } = StaticEnemyState.Idle;
    public event Action<EnemyCombatAgent> Died;

    protected override void Awake()
    {
        base.Awake();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void ResetRunTimeState()
    {
        gameObject.SetActive(true);
        targetReservation.ClearReservations();
        SetCurrentTarget(null);
        unitHealthHandler.RestoreFull();
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        State = StaticEnemyState.Idle;
    }
    
    public void SetGroup(EnemyGroupViewController group)
    {
        Group = group;
    }

    public void Activate()
    {
        if (!IsAlive)
            return;
        attackAgent?.ResetCooldown();
        ResetTargetingTimers();
        State = StaticEnemyState.Attack;
    }

    private void Update()
    {
        if (!IsAlive)
            return;
        if(State != StaticEnemyState.Attack)
            return;

        if (!IsCurrentTargetValidBase())
        {
            TryAcquireTarget();
        }
        else if (ShouldRetarget())
        {
            TryAcquireTarget();
        }
        if (!IsCurrentTargetValidBase())
            return;

        RotateTowardsCurrentTarget(transform);
        if (attackAgent.Tick(Time.deltaTime, currentTarget, AttackOrigin.position))
            SpawnProjectileVisual(currentTarget.transform);
    }

    private void TryAcquireTarget()
    {
        
        ICombatTarget target = allyTargetProvider.GetBestLivingAllyTarget(transform.position, reservationPenalty);

        if (target == null)
        {
            SetCurrentTarget(null);
            nextRetargetTime = Time.time + retargetInterval;
            targetLockUntil = 0f;
            return;
        }

        SetCurrentTarget(target);
        MarkRetargetWindow();
    }

    protected override void HandleDeath()
    {
        Died?.Invoke(this);
        base.HandleDeath();
    }
}
