using UnityEngine;
using Zenject;

public class SquadBarracksSpawner : MonoBehaviour
{
    [Inject] private SquadCombatStateController _squadCombatStateController;
    private CombatUnitFactory unitFactory;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BarracksStats barracksStats;

    private SquadFormationFacade squadFormationFacade;
    private SpawnService<SoldierCombatAgent> spawnService;

    [Inject]
    public void Construct(CombatUnitFactory unitFactory, SquadFormationFacade squadFormationFacade)
    {
        this.unitFactory = unitFactory;
        this.squadFormationFacade = squadFormationFacade;
    }

    private void Awake()
    {
        spawnService = new SpawnService<SoldierCombatAgent>(SpawnSoldier,
            soldier => soldier != null && soldier.gameObject.activeInHierarchy,
            () => barracksStats.SpawnInterval);
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
        var soldier = (SoldierCombatAgent) unitFactory.Create(barracksStats.Unit.UnitID);
        soldier.transform.position = origin.position;
        soldier.transform.rotation = origin.rotation;

        if (squadFormationFacade.RegisterSoldier(soldier))
        {
            soldier.OnDiedEvent += UnRegisterSoldier;
            return soldier;
        }

        Destroy(soldier.gameObject);
        return null;
    }

    private void UnRegisterSoldier(SoldierCombatAgent soldier)
    {
        soldier.OnDiedEvent -= UnRegisterSoldier;
        squadFormationFacade?.UnregisterSoldier(soldier);

    }

    public void UpgradeLevel() => barracksStats.Update();

    private bool CanSpawnInCurrentPhase()
    {
        if (_squadCombatStateController.State != CombatFlowState.IdleInPreparation)
            return false;

        return true;
    }
}


