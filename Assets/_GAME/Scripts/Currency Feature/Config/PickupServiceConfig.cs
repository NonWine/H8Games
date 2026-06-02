using System;
using UnityEngine;

[Serializable]
public class PickupServiceConfig
{
    [SerializeField] private float pickupRadius     = 3f;
    [SerializeField] private int   collectsPerFrame = 4;

    [Header("Carry Stack")]
    [SerializeField] private int   carryMaxVisible  = 15;
    [SerializeField] private float carrySlotSpacing = 0.3f;

    public float PickupRadius     => pickupRadius;
    public int   CollectsPerFrame => collectsPerFrame;
    public int   CarryMaxVisible  => Mathf.Max(1, carryMaxVisible);
    public float CarrySlotSpacing => carrySlotSpacing;
}
