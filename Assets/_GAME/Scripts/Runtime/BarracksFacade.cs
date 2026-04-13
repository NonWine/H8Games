using System;
using UnityEngine;
using Zenject;

public class BarracksFacade : MonoBehaviour, ICombatTarget
{
    [SerializeField] private TeamId teamId = TeamId.Ally;
    [SerializeField] private BarracksStats stats = new();
    [SerializeField] private BarracksUpgradeConfig upgradeConfig = new();
    [SerializeField] private UnitFacade unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private WorldHealthBarView healthBarView;

    private DiContainer diContainer;
    private CurrencyService currencyService;
    private SpawnService<UnitFacade> spawnService;
    private HealthService healthService;
    private BarracksStats runtimeStats;
    private BarracksUpgradeService upgradeService;
    private bool initialized;

    public event Action<BarracksFacade> Destroyed;

    public BarracksUpgradeService UpgradeService => upgradeService;
    public TeamId TeamId => teamId;
    public bool IsAlive => healthService != null && healthService.IsAlive;

    [Inject]
    public void InjectDependencies(DiContainer diContainer, [InjectOptional] CurrencyService currencyService)
    {
        this.diContainer = diContainer;
        this.currencyService = currencyService;
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnDestroy()
    {
        if (healthService != null)
        {
            healthService.HealthChanged -= HandleHealthChanged;
            healthService.Died -= HandleDestroyed;
        }
    }

    private void Update()
    {
        if (!IsAlive)
            return;

        spawnService.Tick(Time.deltaTime, out _);
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

        runtimeStats = new BarracksStats(stats);
        healthService = new HealthService(runtimeStats.MaxHealth);
        upgradeService = new BarracksUpgradeService(runtimeStats,
            upgradeConfig != null ? new BarracksUpgradeConfig(upgradeConfig) : new BarracksUpgradeConfig());
        spawnService = new SpawnService<UnitFacade>(SpawnUnit, unit => unit != null && unit.IsAlive,
            () => runtimeStats.SpawnInterval, () => runtimeStats.MaxAlive);

        healthService.HealthChanged += HandleHealthChanged;
        healthService.Died += HandleDestroyed;
        HandleHealthChanged(healthService.CurrentHealth, healthService.MaxHealth);
        initialized = true;
    }

    private UnitFacade SpawnUnit()
    {
        if (unitPrefab == null)
            return null;

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        UnitFacade unit = diContainer != null
            ? diContainer.InstantiatePrefabForComponent<UnitFacade>(unitPrefab, origin.position, origin.rotation, null)
            : Instantiate(unitPrefab, origin.position, origin.rotation);

        unit.SetRuntimeDependencies(currencyService);
        unit.OverrideTeam(teamId);
        unit.ApplyRuntimeStats(runtimeStats.BuildSpawnStats(unitPrefab.StatsTemplate));
        return unit;
    }

    private void HandleHealthChanged(float current, float max)
    {
        healthBarView?.SetHealth(current, max);
    }

    private void HandleDestroyed()
    {
        Destroyed?.Invoke(this);
        gameObject.SetActive(false);
    }
}
