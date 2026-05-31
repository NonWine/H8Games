using UnityEngine;

public readonly struct PickupCollectedEvent
{
    public readonly string  PickupId;
    public readonly int     Amount;
    public readonly Vector3 WorldPosition;

    public PickupCollectedEvent(string pickupId, int amount, Vector3 worldPosition)
    {
        PickupId      = pickupId;
        Amount        = amount;
        WorldPosition = worldPosition;
    }
}
