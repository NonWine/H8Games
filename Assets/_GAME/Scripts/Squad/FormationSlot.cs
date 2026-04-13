using UnityEngine;

public sealed class FormationSlot
{
    public FormationSlot(int index, Vector3 localOffset)
    {
        Index = index;
        LocalOffset = localOffset;
    }

    public int Index { get; }
    public Vector3 LocalOffset { get; set; }
    public SoldierFollower AssignedSoldier { get; set; }
}
