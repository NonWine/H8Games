using System;
using UnityEngine;

[Serializable]
public class TargetingData
{
    [Min(0.25f)] public float DetectionRadius = 8f;
    [SerializeField, Min(0f)] public float ReservationPenalty = 3f;
    [SerializeField, Min(0.05f)] public float RetargetInterval = 0.35f;
    [SerializeField, Min(0.05f)] public float TargetLockDuration = 0.35f;
    
    public TargetingData()
    {
    }

    public TargetingData(TargetingData source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        DetectionRadius = source.DetectionRadius;
        ReservationPenalty = source.ReservationPenalty;
        RetargetInterval = source.RetargetInterval;
        TargetLockDuration = source.TargetLockDuration;
    }

    public TargetingData Clone()
    {
        return new TargetingData(this);
    }
}
