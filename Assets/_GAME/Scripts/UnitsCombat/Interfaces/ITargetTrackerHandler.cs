using System.Collections.Generic;
using UnityEngine;

public interface ITargetTrackerHandler : IResetModule
{
    ICombatTarget CurrentTarget { get; }
    bool IsCurrentTargetValid();
    void UpdateTarget();
}
