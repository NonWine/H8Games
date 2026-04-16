using UnityEngine;

public class FormationSlot
{
    public int Index { get; }
    public Vector3 LocalOffset { get; set; }
    public SoldierFollower AssignedSoldier { get; set; }
    
    public FormationSlot(int index, Vector3 localOffset)
    {
        Index = index;
        LocalOffset = localOffset;
    }
}
