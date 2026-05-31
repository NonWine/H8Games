using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class RewardOnDeathService : IDisposable
{
    private readonly UnitHealthHandler unitHealthHandler;
    private readonly IPickupService pickupService;
    private readonly Transform owner;
    private readonly string pickupId;
    private readonly int reward;

    public RewardOnDeathService(
        UnitHealthHandler unitHealthHandler,
        IPickupService pickupService,
        Transform owner,
        string pickupId,
        int reward)
    {
        this.unitHealthHandler = unitHealthHandler;
        this.pickupService     = pickupService;
        this.owner             = owner;
        this.pickupId          = pickupId;
        this.reward            = reward;

        this.unitHealthHandler.Died += HandleDeath;
    }

    public void Dispose()
    {
        unitHealthHandler.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        pickupService.SpawnAsync(new PickupSpawnRequest(pickupId, reward, owner.position)).Forget();
    }
}
