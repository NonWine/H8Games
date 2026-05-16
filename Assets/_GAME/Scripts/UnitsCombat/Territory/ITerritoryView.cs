using System.Collections.Generic;
using UnityEngine;

public interface ITerritoryView
{
    void Refresh(IReadOnlyList<Vector3> positions, TerritoryConfig config);
    void Clear();
    void SetAnimating(bool active);
    void SetCombatAlert(bool active);
}
