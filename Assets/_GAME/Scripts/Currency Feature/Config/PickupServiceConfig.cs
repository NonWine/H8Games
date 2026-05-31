using System;
using UnityEngine;

[Serializable]
public sealed class PickupServiceConfig
{
    [SerializeField] private float pickupRadius     = 3f;
    [SerializeField] private int   collectsPerFrame = 4;

    public float PickupRadius     => pickupRadius;
    public int   CollectsPerFrame => collectsPerFrame;
}
