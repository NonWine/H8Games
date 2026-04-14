using UnityEngine;
using Zenject;

[RequireComponent(typeof(SoldierFollower))]
public sealed class SoldierCombatAgent : BaseCombatUnitView, ICombatTarget, ITargetReservation
{
    [SerializeField] private bool autoRegisterOnStart = true;
    [SerializeField] private TeamId teamId = TeamId.Ally;
    [SerializeField] private UnitStats stats = new();
    [SerializeField] private Transform attackPoint;
    [SerializeField] private WorldHealthBarView healthBarView;
    [SerializeField] private SimpleProjectileView projectilePrefab;
    [SerializeField, Min(0f)] private float reservationPenalty = 3f;
    [SerializeField, Min(0.05f)] private float retargetInterval = 0.35f;
    [SerializeField, Min(0.05f)] private float targetLockDuration = 0.35f;
    
    private SquadCombatCoordinator squadCombatCoordinator;
    private AttackService attackService;
    private HealthService healthService;
    private UnitStats runtimeStats;
    private ICombatTarget currentTarget;
    private float nextRetargetTime;
    private float targetLockUntil;
    private bool initialized;
    private readonly System.Collections.Generic.HashSet<Component> reservationAttackers = new();

    public TeamId TeamId => teamId;
    public int CurrentWeight { get; set; }
    public bool IsAlive => healthService != null && healthService.IsAlive;
    public SoldierCombatState State { get; private set; } = SoldierCombatState.Idle;
    public Transform AttackOrigin => attackPoint != null ? attackPoint : transform;
    public UnitStats RuntimeStats => runtimeStats;
    public int ReservationCount => reservationAttackers.Count;

    [Inject]
    public void Construct(SquadCombatCoordinator squadCombatCoordinator)
    {
        this.squadCombatCoordinator = squadCombatCoordinator;
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        if (autoRegisterOnStart && squadCombatCoordinator != null)
            squadCombatCoordinator.RegisterSoldier(this);
    }

    private void Update()
    {
        if (!IsAlive || squadCombatCoordinator == null)
            return;

        EnemyGroupFacade currentGroup = squadCombatCoordinator.CurrentTargetGroup;
        if (currentGroup == null || currentGroup.State != EnemyGroupState.Activated)
        {
            SetCurrentTarget(null);
            State = SoldierCombatState.Idle;
            return;
        }

        if (!IsCurrentTargetValid(currentGroup))
        {
            TryAcquireTarget(currentGroup);
        }
        else if (Time.time >= nextRetargetTime && Time.time >= targetLockUntil)
        {
            TryAcquireTarget(currentGroup);
        }

        if (!IsCurrentTargetValid(currentGroup))
        {
            SetCurrentTarget(null);
            State = SoldierCombatState.Idle;
            return;
        }

        Vector3 direction = currentTarget.transform.position - transform.position;
        direction.y = 0f;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);


        State = SoldierCombatState.Attack;
        if (attackService.Tick(Time.deltaTime, currentTarget))
            SpawnProjectileVisual(currentTarget.transform);
    }

    private void OnDisable()
    {
        SetCurrentTarget(null);
        squadCombatCoordinator?.UnregisterSoldier(this);
    }

    private void OnDestroy()
    {
        if (healthService == null)
            return;

        healthService.HealthChanged -= HandleHealthChanged;
        healthService.Died -= HandleDeath;
    }

    public void GetDamage(float damage, Vector3 sourceWorldPosition)
    {
        EnsureInitialized();
        healthService.ApplyDamage(damage);
        SetEmissionHitFlash();
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        runtimeStats = new UnitStats(stats);
        healthService = new HealthService(runtimeStats.MaxHealth);
        attackService = new AttackService(
            () => runtimeStats.Damage,
            () => runtimeStats.AttackCooldown,
            () => AttackOrigin.position);
        healthService.HealthChanged += HandleHealthChanged;
        healthService.Died += HandleDeath;
        HandleHealthChanged(healthService.CurrentHealth, healthService.MaxHealth);
        initialized = true;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        healthBarView?.SetHealth(currentHealth, maxHealth);
    }

    private void HandleDeath()
    {
        State = SoldierCombatState.Dead;
        gameObject.SetActive(false);
    }

    private void SpawnProjectileVisual(Transform target)
    {
        if (projectilePrefab == null || target == null)
            return;

        SimpleProjectileView projectile = Instantiate(projectilePrefab, AttackOrigin.position, Quaternion.identity);
        projectile.Launch(target, runtimeStats.ProjectileSpeed);
    }

    public bool TryRegisterAttacker(Component attacker)
    {
        if (attacker == null)
            return false;

        return reservationAttackers.Add(attacker);
    }

    public bool TryUnregisterAttacker(Component attacker)
    {
        if (attacker == null)
            return false;

        return reservationAttackers.Remove(attacker);
    }

    public void ClearReservations()
    {
        reservationAttackers.Clear();
    }

    private bool TryAcquireTarget(EnemyGroupFacade currentGroup)
    {
        if (currentGroup == null)
        {
            SetCurrentTarget(null);
            return false;
        }

        ICombatTarget target = currentGroup.GetBestLivingEnemyTarget(transform.position, reservationPenalty);
        if (target == null)
        {
            nextRetargetTime = Time.time + retargetInterval;
            targetLockUntil = 0f;
            return false;
        }

        SetCurrentTarget(target);
        nextRetargetTime = Time.time + retargetInterval;
        targetLockUntil = Time.time + targetLockDuration;
        return true;
    }

    private void SetCurrentTarget(ICombatTarget newTarget)
    {
        if (ReferenceEquals(currentTarget, newTarget))
            return;

        ReleaseCurrentTarget();
        currentTarget = newTarget;

        if (currentTarget is Component targetComponent)
            RegisterCurrentTarget(targetComponent);

        if (currentTarget == null)
        {
            nextRetargetTime = Time.time + retargetInterval;
            targetLockUntil = 0f;
        }
    }

    private void RegisterCurrentTarget(Component targetComponent)
    {
        if (targetComponent is not ITargetReservation reservationTarget)
            return;

        reservationTarget.TryRegisterAttacker(this);
    }

    private void ReleaseCurrentTarget()
    {
        if (currentTarget is not Component targetComponent)
            return;

        if (targetComponent is ITargetReservation reservationTarget)
            reservationTarget.TryUnregisterAttacker(this);
    }

    private bool IsCurrentTargetValid(EnemyGroupFacade currentGroup)
    {
        if (currentTarget == null || !currentTarget.IsAlive)
            return false;

        if (currentTarget is not Component targetComponent || !targetComponent.gameObject.activeInHierarchy)
            return false;

        if (currentGroup != null && !currentGroup.ContainsEnemy(currentTarget))
            return false;

        return true;

    }
}

