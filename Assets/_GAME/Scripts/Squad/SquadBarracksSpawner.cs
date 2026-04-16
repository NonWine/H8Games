using UnityEngine;
using Zenject;

public class SquadBarracksSpawner : MonoBehaviour
{
    [Inject] private CombatStateController combatStateController;
    [SerializeField] private SoldierFollower soldierPrefab;
    [SerializeField] private Transform spawnPoint;
    [Min(0.15f)]
    [SerializeField] private float spawnInterval = 2.5f;

    private DiContainer container;
    private SquadFormationFacade squadFormationFacade;
    private SpawnService<SoldierCombatAgent> spawnService;

    [Inject]
    public void Construct(DiContainer container, SquadFormationFacade squadFormationFacade)
    {
        this.container = container;
        this.squadFormationFacade = squadFormationFacade;
    }

    private void Awake()
    {
        spawnService = new SpawnService<SoldierCombatAgent>(SpawnSoldier,
            soldier => soldier != null && soldier.gameObject.activeInHierarchy,
            () => spawnInterval);
    }

    private void Update()
    {
        if (!CanSpawnInCurrentPhase())
            return;

        spawnService.Tick(Time.deltaTime, out _);
    }

    private SoldierCombatAgent SpawnSoldier()
    {
        if (!squadFormationFacade.HasFreeSlot || !CanSpawnInCurrentPhase())
            return null;

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        SoldierCombatAgent soldier = container.InstantiatePrefabForComponent<SoldierCombatAgent>(
            soldierPrefab,
            origin.position,
            origin.rotation,
            null);

        if (squadFormationFacade.RegisterSoldier(soldier))
            return soldier;

        Destroy(soldier.gameObject);
        return null;
    }

    private bool CanSpawnInCurrentPhase()
    {
        if (combatStateController.State != CombatFlowState.IdleInPreparation)
            return false;

        return true;
    }
}
