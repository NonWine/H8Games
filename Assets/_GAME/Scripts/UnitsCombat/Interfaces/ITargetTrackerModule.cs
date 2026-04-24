using System.Collections.Generic;
using UnityEngine;

public interface ITargetTrackerModule : IResetModule
{
    ICombatTarget CurrentTarget { get; }
    bool IsCurrentTargetValid();
    void UpdateTarget(UnitState state);
}
