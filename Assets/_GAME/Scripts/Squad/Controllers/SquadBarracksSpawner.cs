using DG.Tweening;
using UnityEngine;
using Zenject;

public class SquadBarracksSpawner : MonoBehaviour
{
    [Inject] private SquadCombatStateController _squadCombatStateController;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BarracksStats barracksStats;
    [SerializeField] private GameObject[] barracks;
    [SerializeField] private BarracksUpgradeRevealView upgradeReveal;

    [Header("Upgrade Cinematic")]
    [SerializeField] private Transform cinematicFocusPoint;

    [Header("Spawn Kick")]
    [SerializeField, Min(0f)] private float spawnKickStrength = 0.12f;
    [SerializeField, Min(0.01f)] private float spawnKickDuration = 0.22f;
    [SerializeField, Min(0)] private int spawnKickVibrato = 6;
    [SerializeField, Range(0f, 1f)] private float spawnKickElasticity = 0.6f;

    private SoldierFactory soldierFactory;
    private SquadFormationFacade squadFormationFacade;
    private ICinematicCameraFocus cinematicCameraFocus;
    private SpawnService<SoldierCombatAgentController> spawnService;
    private Tween spawnKickTween;
    private GameObject pendingPreviousModel;
    private GameObject pendingNextModel;

    [Inject]
    public void Construct(
        SoldierFactory soldierFactory,
        SquadFormationFacade squadFormationFacade,
        ICinematicCameraFocus cinematicCameraFocus)
    {
        this.soldierFactory = soldierFactory;
        this.squadFormationFacade = squadFormationFacade;
        this.cinematicCameraFocus = cinematicCameraFocus;
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

        if (spawnService.Tick(Time.deltaTime, out _))
        {
            PlaySpawnKick();
        }
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

        // The squad owns the soldier's lifetime once it holds a slot: it subscribes to
        // Died and releases the slot itself, so nothing here can free a slot twice.
        if (squadFormationFacade.RegisterSoldier(soldier))
        {
            return soldier;
        }

        soldierFactory.Release(soldier);
        return null;
    }

    public void UpgradeLevel()
    {
        GameObject previousModel = barracksStats?.BarrackLevelData.UnitModel;
        barracksStats?.Update();
        GameObject nextModel = barracksStats?.BarrackLevelData.UnitModel;

        if (upgradeReveal == null || previousModel == null || nextModel == null || previousModel == nextModel)
        {
            SetBarrackView();
            return;
        }

        pendingPreviousModel = previousModel;
        pendingNextModel = nextModel;

        Transform focusPoint = cinematicFocusPoint != null ? cinematicFocusPoint : nextModel.transform;
        Tween focusTween = cinematicCameraFocus.Focus(focusPoint);

        if (focusTween == null)
        {
            PlayReveal();
            return;
        }

        focusTween.OnComplete(PlayReveal);
    }

    private void PlayReveal()
    {
        GameObject previousModel = pendingPreviousModel;
        GameObject nextModel = pendingNextModel;
        pendingPreviousModel = null;
        pendingNextModel = null;

        if (this == null || upgradeReveal == null || previousModel == null || nextModel == null)
        {
            ReleaseCinematic();
            return;
        }

        upgradeReveal.Play(previousModel, nextModel, ReleaseCinematic);
    }

    private void ReleaseCinematic()
    {
        cinematicCameraFocus.Release();
    }

    private void PlaySpawnKick()
    {
        GameObject model = barracksStats?.BarrackLevelData.UnitModel;
        if (model == null || spawnKickStrength <= 0f)
        {
            return;
        }

        Transform modelTransform = model.transform;

        spawnKickTween?.Kill();
        modelTransform.localScale = Vector3.one;
        spawnKickTween = modelTransform
            .DOPunchScale(Vector3.one * spawnKickStrength, spawnKickDuration, spawnKickVibrato, spawnKickElasticity)
            .SetLink(modelTransform.gameObject);
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
