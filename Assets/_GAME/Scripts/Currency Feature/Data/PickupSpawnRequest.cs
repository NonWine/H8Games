using UnityEngine;

public readonly struct PickupSpawnRequest
{
    public readonly string   PickupId;
    public readonly int      Amount;
    public readonly Vector3  Position;
    public readonly Vector3? ScatterDirection;

    public PickupSpawnRequest(string pickupId, int amount, Vector3 position, Vector3? scatterDirection = null)
    {
        PickupId         = pickupId;
        Amount           = amount;
        Position         = position;
        ScatterDirection = scatterDirection;
    }
}
