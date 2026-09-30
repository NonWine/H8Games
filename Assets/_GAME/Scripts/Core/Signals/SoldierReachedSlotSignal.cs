// Feedback only: a soldier settled onto his formation slot. Nothing in the
// combat or formation rules listens to this.
public class SoldierReachedSlotSignal
{
    public readonly int SlotIndex;

    public SoldierReachedSlotSignal(int slotIndex)
    {
        SlotIndex = slotIndex;
    }
}
