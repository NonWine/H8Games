using System;
using UnityEngine;

[RequireComponent(typeof(EnemyCombatAgent))]
public sealed class StaticEnemyAgent : BaseCombatUnitView, ICombatTarget
{
    [SerializeField] private TeamId teamId = TeamId.Enemy;
    [SerializeField] private UnitStats stats = new();
    [SerializeField] private Transform attackPoint;
    [SerializeField] private WorldHealthBarView healthBarView;
    [SerializeField] private SimpleProjectileView projectilePrefab;

    private EnemyCombatAgent combatAgent;
    private HealthService healthService;
    private UnitStats runtimeStats;
    private bool initialized;

    public event Action<StaticEnemyAgent> Died;

    public TeamId TeamId => teamId;
    public bool IsAlive => healthService.IsAlive;
    public Transform AttackOrigin => attackPoint != null ? attackPoint : transform;
    public UnitStats RuntimeStats => runtimeStats;
    public EnemyGroupFacade Group { get; private set; }
    public StaticEnemyState State { get; private set; } = StaticEnemyState.Idle;
    public int CurrentWeight { get; set; }

    private void Awake()
    {
        combatAgent = GetComponent<EnemyCombatAgent>();
        EnsureInitialized();
        combatAgent.Initialize(this);
    }

    private void OnDestroy()
    {
        if (healthService == null)
            return;

        healthService.HealthChanged -= HandleHealthChanged;
        healthService.Died -= HandleDeath;
    }

    public void SetGroup(EnemyGroupFacade group)
    {
        Group = group;
    }

    public void Activate(SquadCombatCoordinator coordinator)
    {
        if (!IsAlive)
            return;

        State = StaticEnemyState.Attack;
        combatAgent.Activate(coordinator);
    }

    public void GetDamage(float damage, Vector3 sourceWorldPosition)
    {
        EnsureInitialized();
        healthService.ApplyDamage(damage);
        SetEmissionHitFlash();
    }

    public void SpawnProjectileVisual(Transform target)
    {
        if (projectilePrefab == null || target == null)
            return;

        SimpleProjectileView projectile = Instantiate(projectilePrefab, AttackOrigin.position, Quaternion.identity);
        projectile.Launch(target, runtimeStats.ProjectileSpeed);
    }

    public void ResetRuntimeState()
    {
        EnsureInitialized();
        if (combatAgent == null) combatAgent = GetComponent<EnemyCombatAgent>();
        combatAgent.Deactivate();
        healthService.RestoreFull();

        State = StaticEnemyState.Idle;
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        runtimeStats = new UnitStats(stats);
        healthService = new HealthService(runtimeStats.MaxHealth);
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
        State = StaticEnemyState.Dead;
        combatAgent.Deactivate();
        Died?.Invoke(this);
        gameObject.SetActive(false);
    }
}
