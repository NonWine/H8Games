using System;
using UnityEngine;
using Zenject;
[RequireComponent(typeof(BaseCombatUnitView))]
public abstract class BaseTargetingCombatAgent : MonoBehaviour, ICombatTarget
{
    [SerializeField] private SimpleProjectileView projectilePrefab;
    [SerializeField] private UnitStats stats = new();
    [SerializeField] private Transform attackPoint;
    [SerializeField, Min(0f)] protected float reservationPenalty = 3f;
    [SerializeField, Min(0.05f)] protected float retargetInterval = 0.35f;
    [SerializeField, Min(0.05f)] protected float targetLockDuration = 0.35f;
    [Inject] protected TargetReservation targetReservation;
    protected UnitAttackAgentHandler attackAgent;
    protected UnitHealthHandler unitHealthHandler;
    protected ICombatTarget currentTarget;
    protected BaseCombatUnitView baseCombatUnitView;
    protected float targetLockUntil;
    protected float nextRetargetTime;
    protected AttackRuntimeModel attackRuntimeModel;

    public Transform AttackOrigin => attackPoint;

    private void Awake()
    {
        baseCombatUnitView = GetComponent<BaseCombatUnitView>();
        attackRuntimeModel = new AttackRuntimeModel(stats);
        attackAgent = new UnitAttackAgentHandler(attackRuntimeModel);
        unitHealthHandler = new UnitHealthHandler(stats.MaxHealth);
        unitHealthHandler.Died += HandleDeath;
    }

    protected bool ShouldRetarget()
    {
        return Time.time >= nextRetargetTime && Time.time >= targetLockUntil;
    }
    
    protected void MarkRetargetWindow()
    {
        nextRetargetTime = Time.time + retargetInterval;
        targetLockUntil = Time.time + targetLockDuration;
    }

    protected void SpawnProjectileVisual(Transform target)
    {
        if (projectilePrefab == null || target == null)
            return;

        SimpleProjectileView projectile = Instantiate(projectilePrefab, AttackOrigin.position, Quaternion.identity);
        projectile.Launch(target, stats.ProjectileSpeed);
    }

    protected void ResetTargetingTimers()
    {
        nextRetargetTime = 0f;
        targetLockUntil = 0f;
    }

    protected void SetCurrentTarget(ICombatTarget newTarget)
    {
        if (ReferenceEquals(currentTarget, newTarget))
            return;

        ReleaseCurrentTarget();
        currentTarget = newTarget;

        if (currentTarget is Component targetComponent &&
            targetComponent is ITargetReservation reservationTarget)
        {
            reservationTarget.TryRegisterAttacker(this);
        }

        if (currentTarget == null)
        {
            nextRetargetTime = Time.time + retargetInterval;
            targetLockUntil = 0f;
        }
    }

    private void ReleaseCurrentTarget()
    {
        if (currentTarget is Component targetComponent &&
            targetComponent is ITargetReservation reservationTarget)
        {
            reservationTarget.TryUnregisterAttacker(this);
        }
    }

    protected bool IsCurrentTargetValidBase(EnemyGroupFacade currentGroup = null)
    {
        if (currentTarget == null || !currentTarget.IsAlive)
            return false;

        if (currentTarget is not Component targetComponent || !targetComponent.gameObject.activeInHierarchy)
            return false;

        if (currentGroup != null && !currentGroup.ContainsEnemy(currentTarget))
            return false;

        return true;
    }

    protected bool RotateTowardsCurrentTarget(Transform selfTransform)
    {
        if (currentTarget == null)
            return false;

        Vector3 direction = currentTarget.transform.position - selfTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return false;

        selfTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        return true;
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        SetCurrentTarget(null);
        targetReservation.ClearReservations();
    }

    protected virtual void OnDestroy()
    {
        if (unitHealthHandler == null)
            return;

        unitHealthHandler.Died -= HandleDeath;
    }

    public void GetDamage(float damage, Vector3 sourceWorldPosition)
    {
        unitHealthHandler.ApplyDamage(damage);
        baseCombatUnitView.SetEmissionHitFlash();
    }

    protected virtual void HandleDeath()
    {
       gameObject.SetActive(false);
    }

    public bool IsAlive => unitHealthHandler.IsAlive;
}