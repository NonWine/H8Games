using UnityEngine;

public readonly struct PickupMagnet
{
    public readonly Vector3   Position;
    public readonly float     Radius;
    public readonly Transform Anchor;

    public PickupMagnet(Vector3 position, float radius, Transform anchor)
    {
        Position = position;
        Radius   = radius;
        Anchor   = anchor;
    }
}
