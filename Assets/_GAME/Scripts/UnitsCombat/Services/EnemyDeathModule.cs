using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyDeathModule : UnitDeathModule
{
    private readonly GameObject ownerObject;
    private readonly IPickupService pickupService;
    private readonly string pickupId;
    private readonly int reward;

    public EnemyDeathModule(
        GameObject ownerObject,
        IPickupService pickupService,
        string pickupId,
        int reward,
        int disableDelayMs = 5000)
        : base(ownerObject, disableDelayMs)
    {
        this.ownerObject   = ownerObject;
        this.pickupService = pickupService;
        this.pickupId      = pickupId;
        this.reward        = reward;
    }

    public override async UniTask HandleDeathAsync(Action beforeDisable = null)
    {
        var position = ownerObject.transform.position;

        await base.HandleDeathAsync(beforeDisable);
        await pickupService.SpawnAsync(new PickupSpawnRequest(pickupId, reward, position));
    }
}
