using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SquadRootView : MonoBehaviour
{
    [field: SerializeField, Min(0.01f)] public float TargetReachThreshold { get; private set; } = 0.1f;

    public Vector3 HomePosition { get; private set; }
    public Quaternion HomeRotation { get; private set; }

    private void Awake()
    {
        HomePosition = transform.position;
        HomeRotation = transform.rotation;
    }
}


