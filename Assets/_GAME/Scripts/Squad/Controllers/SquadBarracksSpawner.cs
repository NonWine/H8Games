using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class SquadBarracksSpawner : MonoBehaviour
{
    [Inject] private SquadCombatStateController _squadCombatStateController;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BarracksStats barracksStats;
    [SerializeField] private GameObject[] barracks;

    private SoldierFactory soldierFactory;
    private SquadFormationFacade squadFormationFacade;
    private SpawnService<SoldierCombatAgentController> spawnService;

    [Inject]
    public void Construct(SoldierFactory soldierFactory, SquadFormationFacade squadFormationFacade)
    {
        this.soldierFactory = soldierFactory;
        this.squadFormationFacade = squadFormationFacade;
    }

    private void Awake()
    {
        barracksStats?.ResetRuntimeState();
        spawnService = new SpawnService<SoldierCombatAgentController>(
            SpawnSoldier,
            soldier => soldier != null && soldier.IsAlive,
            () => barracksStats.SpawnInterval);
    }

    private void Start()
    {
        SetBarrackView();
    }

    private void Update()
    {
        if (!CanSpawnInCurrentPhase())
        {
            return;
        }

        spawnService.Tick(Time.deltaTime, out _);
    }

    private SoldierCombatAgentController SpawnSoldier()
    {
        if (!squadFormationFacade.HasFreeSlot)
        {
            return null;
        }

        if (!CanSpawnInCurrentPhase())
        {
            return null;
        }

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        SoldierCombatAgentController soldier = soldierFactory.Create(barracksStats.Unit.UnitID, origin.position, origin.rotation);

        if (squadFormationFacade.RegisterSoldier(soldier))
        {
            Action diedHandler = null;
            diedHandler = () =>
            {
                soldier.Died -= diedHandler;
                UnRegisterSoldier(soldier);
            };

            soldier.Died += diedHandler;
            return soldier;
        }

        soldierFactory.Release(soldier);
        return null;
    }

    private void UnRegisterSoldier(SoldierCombatAgentController soldier)
    {
        squadFormationFacade?.UnregisterSoldier(soldier);
    }

    public void UpgradeLevel()
    {
        barracksStats?.Update();
        SetBarrackView();
    }

    private void SetBarrackView()
    {
        foreach (GameObject barrack in barracks)
        {
            barrack.gameObject.SetActive(false);
        }

        var newModel = barracksStats.BarrackLevelData.UnitModel;
        newModel.transform.localScale = Vector3.one;
        newModel.gameObject.SetActive(true);
        newModel.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack);
        newModel.transform.DOScale(1f, 0.15f).SetEase(Ease.Linear).SetDelay(0.25f);
    }

    private bool CanSpawnInCurrentPhase()
    {
        return _squadCombatStateController.State == CombatFlowState.IdleInPreparation;
    }
}
