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

    [Inject]
    public void Construct(DiContainer container, SquadRoot squadRoot)
    {
        this.container = container;
        this.squadRoot = squadRoot;
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
        spawnService.Tick(Time.deltaTime, out _);
    }

    private SoldierFollower SpawnSoldier()
    {
        if (soldierPrefab == null || squadRoot == null || !squadRoot.HasFreeSlot)
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
}
