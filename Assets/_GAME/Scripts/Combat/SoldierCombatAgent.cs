using UnityEngine;
using Zenject;

[RequireComponent(typeof(SoldierFollower))]
public sealed class SoldierCombatAgent : MonoBehaviour, ICombatTarget
{
    [SerializeField] private bool autoRegisterOnStart = true;
    [SerializeField] private TeamId teamId = TeamId.Ally;
    [SerializeField] private UnitStats stats = new();
    [SerializeField] private Transform attackPoint;
    [SerializeField] private WorldHealthBarView healthBarView;
    [SerializeField] private SimpleProjectileView projectilePrefab;

    private SquadCombatCoordinator squadCombatCoordinator;
    private AttackService attackService;
    private HealthService healthService;
    private UnitStats runtimeStats;
    private bool initialized;

    public TeamId TeamId => teamId;
    public bool IsAlive => healthService != null && healthService.IsAlive;
    public SoldierCombatState State { get; private set; } = SoldierCombatState.Idle;
    public Transform AttackOrigin => attackPoint != null ? attackPoint : transform;
    public UnitStats RuntimeStats => runtimeStats;

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
            State = SoldierCombatState.Idle;
            return;
        }

        ICombatTarget target = currentGroup.GetClosestLivingEnemy(transform.position);
        if (target == null || !target.IsAlive)
        {
            State = SoldierCombatState.Idle;
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            State = SoldierCombatState.Idle;
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        float sqrRange = runtimeStats.AttackRange * runtimeStats.AttackRange;
        if (direction.sqrMagnitude > sqrRange)
        {
            State = SoldierCombatState.Idle;
            return;
        }

        State = SoldierCombatState.Attack;
        if (attackService.Tick(Time.deltaTime, target))
            SpawnProjectileVisual(target.transform);
    }

    private void OnDisable()
    {
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
}
