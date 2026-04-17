using System;
using UnityEngine;

public class UnitAttackAnimationEventRelay : MonoBehaviour
{
    public event Action AttackTriggered;

    public void RaiseAttackEvent()
    {
        AttackTriggered?.Invoke();
    }
}
