using System;
using UnityEngine;

public interface IMoveProvider
{
    public void SetTarget(Vector3 worldPosition, Action onReached = null);
}