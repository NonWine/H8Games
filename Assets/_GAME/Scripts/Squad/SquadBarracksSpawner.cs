using UnityEngine;
using Zenject;

public sealed class SquadBarracksSpawner : MonoBehaviour
{
    [SerializeField] private SoldierFollower soldierPrefab;
    [SerializeField] private Transform spawnPoint;
    [Min(0.15f)]
    [SerializeField] private float spawnInterval = 2.5f;

    private DiContainer container;
    private SquadRoot squadRoot;
    private SpawnService<SoldierFollower> spawnService;
    private GamePhaseService gamePhaseService;

    [Inject]
    public void Construct(
        DiContainer container,
        SquadRoot squadRoot,
        [InjectOptional] GamePhaseService gamePhaseService)
    {
        this.container = container;
        this.squadRoot = squadRoot;
        this.gamePhaseService = gamePhaseService;
    }

    private void Awake()
    {
        spawnService = new SpawnService<SoldierFollower>(
            SpawnSoldier,
            soldier => soldier != null && soldier.gameObject.activeInHierarchy,
            () => spawnInterval);
    }

    private void Update()
    {
        if (!CanSpawnInCurrentPhase())
            return;

        spawnService.Tick(Time.deltaTime, out _);
    }

    private SoldierFollower SpawnSoldier()
    {
        if (soldierPrefab == null || squadRoot == null || !squadRoot.HasFreeSlot || !CanSpawnInCurrentPhase())
            return null;

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        SoldierFollower soldier = container.InstantiatePrefabForComponent<SoldierFollower>(
            soldierPrefab,
            origin.position,
            origin.rotation,
            null);

        if (squadRoot.RegisterSoldier(soldier))
            return soldier;

        Destroy(soldier.gameObject);
        return null;
    }

    private bool CanSpawnInCurrentPhase()
    {
        if (gamePhaseService != null && !gamePhaseService.IsPreparation)
            return false;

        return true;
    }
}
