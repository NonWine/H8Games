using UnityEngine;

public interface ITargetSelectionCandidate : ICombatTarget
{
    Vector3 Position { get; }
    bool IsAlive { get; }
    int ReservationCount { get; }
}
