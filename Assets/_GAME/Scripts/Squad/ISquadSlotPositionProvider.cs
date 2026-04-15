using UnityEngine;

public interface ISquadSlotPositionProvider
{
    public Vector3 GetSlotWorldPosition(FormationSlot slot);
    public Vector3 GetSlotWorldPosition(int slotIndex);
}