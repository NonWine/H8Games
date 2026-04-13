using System;
using UnityEngine;
using Zenject;

public class BarracksFacade : MonoBehaviour
{
    [SerializeField] private BarracksStats stats = new();
    [SerializeField] private BarracksUpgradeConfig upgradeConfig = new();
    [SerializeField] private UnitFacade unitPrefab;
    [SerializeField] private Transform spawnPoint;

    private DiContainer diContainer;
    private CurrencyService currencyService;
    private SpawnService<UnitFacade> spawnService;
    private BarracksStats runtimeStats;
    private BarracksUpgradeService upgradeService;
    private bool initialized;

    public event Action<BarracksFacade> Destroyed;

    public BarracksUpgradeService UpgradeService => upgradeService;

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
    
    private void Update()
    {
        spawnService.Tick(Time.deltaTime, out _);
    }

    public void GetDamage(float damage, Vector3 sourceWorldPosition)
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        runtimeStats = new BarracksStats(stats);
        upgradeService = new BarracksUpgradeService(runtimeStats, upgradeConfig != null ? new BarracksUpgradeConfig(upgradeConfig) : new BarracksUpgradeConfig());
        spawnService = new SpawnService<UnitFacade>(SpawnUnit, unit => unit.IsAlive, () => runtimeStats.SpawnInterval);
        initialized = true;
    }

    private UnitFacade SpawnUnit()
    {
        if (unitPrefab == null)
            return null;

        UnitFacade unit = diContainer.InstantiatePrefabForComponent<UnitFacade>(unitPrefab, spawnPoint.position, spawnPoint.rotation, null);

        unit.SetRuntimeDependencies(currencyService);
        return unit;
    }
    
}
