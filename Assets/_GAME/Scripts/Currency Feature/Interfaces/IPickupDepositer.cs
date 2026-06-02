using System;
using UnityEngine;

public interface IPickupDepositer
{
    void TossDeposit(string pickupId, Vector3 origin, Transform target, Action onArrived);
}
