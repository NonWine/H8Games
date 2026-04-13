using System;
using UnityEngine;
using Zenject;

public class UnitFacade : MonoBehaviour, ICombatTarget
{
    [SerializeField] private TeamId teamId;
    [SerializeField] private UnitStats stats = new();
    [SerializeField] private Transform attackPoint;
    [SerializeField] private WorldHealthBarView healthBarView;
    [SerializeField] private SimpleProjectileView projectilePrefab;
    [SerializeField] private LayerMask detectionMask = ~0;

    private CurrencyService currencyService;
    private TargetSelector targetSelector;
    private HealthService healthService;
    private AttackService attackService;
    private RewardOnDeathService rewardOnDeathService;
    private UnitStats runtimeStats;
    private ICombatTarget currentTarget;
    private bool initialized;

    public event Action<UnitFacade> Died;

    public TeamId TeamId => teamId;
    public bool IsAlive => healthService != null && healthService.IsAlive;
    public Transform AttackOrigin => attackPoint != null ? attackPoint : transform;

    [Inject]
    public void InjectDependencies([InjectOptional] CurrencyService currencyService)
    {
        this.currencyService = currencyService;
    }

    public void SetRuntimeDependencies(CurrencyService currencyService)
    {
        this.currencyService = currencyService;

        if (initialized)
        {
            initialized = false;
            EnsureInitialized();
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnDestroy()
    {
        if (healthService != null)
        {
            healthService.HealthChanged -= OnHealthChanged;
            healthService.Died -= OnDeath;
        }

        rewardOnDeathService?.Dispose();
    }

    private void Update()
    {
        if (!IsAlive) return;

        if (currentTarget == null || !currentTarget.IsAlive || !IsWithinDetection(currentTarget.transform))
        {
            currentTarget = targetSelector.GetClosestEnemy(transform, runtimeStats.DetectionRadius, teamId, detectionMask);
        }

        if (currentTarget == null)
            return;

        Vector3 direction = currentTarget.transform.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f) transform.forward = direction.normalized;

        float distance = direction.magnitude;
        if (distance > runtimeStats.AttackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.transform.position, runtimeStats.MoveSpeed * Time.deltaTime);
            return;
        }

        if (attackService.Tick(Time.deltaTime, currentTarget))
            SpawnProjectileVisual(currentTarget.transform);
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

        if (healthService != null)
        {
            healthService.HealthChanged -= OnHealthChanged;
            healthService.Died -= OnDeath;
        }

        runtimeStats ??= new UnitStats(stats);
        targetSelector = new TargetSelector();
        healthService = new HealthService(runtimeStats.MaxHealth);
        attackService = new AttackService(() => runtimeStats.Damage, () => runtimeStats.AttackCooldown,
            () => AttackOrigin.position);
        rewardOnDeathService?.Dispose();
        rewardOnDeathService = currencyService != null && runtimeStats.DeathReward > 0
            ? new RewardOnDeathService(healthService, currencyService, runtimeStats.DeathReward)
            : null;

        healthService.HealthChanged += OnHealthChanged;
        healthService.Died += OnDeath;
        OnHealthChanged(healthService.CurrentHealth, healthService.MaxHealth);
        initialized = true;
    }

    private bool IsWithinDetection(Transform target)
    {
        return target != null &&
               (target.position - transform.position).sqrMagnitude <=
               runtimeStats.DetectionRadius * runtimeStats.DetectionRadius;
    }

    private void OnHealthChanged(float current, float max)
    {
        healthBarView?.SetHealth(current, max);
    }

    private void OnDeath()
    {
        Died?.Invoke(this);
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
