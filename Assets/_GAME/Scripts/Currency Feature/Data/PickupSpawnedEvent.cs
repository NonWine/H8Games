using UnityEngine;

public readonly struct PickupSpawnedEvent
{
    public readonly string  PickupId;
    public readonly int     Amount;
    public readonly Vector3 WorldPosition;

    public PickupSpawnedEvent(string pickupId, int amount, Vector3 worldPosition)
    {
        PickupId      = pickupId;
        Amount        = amount;
        WorldPosition = worldPosition;
    }
}
