using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class StackPickupCarrySink : IPickupCarrySink
{
    private readonly IPickupCarryAnchorProvider anchorProvider;
    private readonly PickupServiceConfig        config;
    private readonly List<PickupItemController> stack = new();

    public event Action<PickupItemController> Evicted;

    public StackPickupCarrySink(IPickupCarryAnchorProvider anchorProvider, PickupServiceConfig config)
    {
        this.anchorProvider = anchorProvider;
        this.config         = config;
    }

    public bool TryAttach(PickupItemController controller, out Transform anchor, out Vector3 localPos, out Quaternion localRot)
    {
        localPos = Vector3.zero;
        localRot = Quaternion.identity;

        if (!anchorProvider.TryGetAnchor(out anchor))
            return false;

        stack.Add(controller);

        if (stack.Count > config.CarryMaxVisible)
            EvictOldest();

        ReslotCarriedItems(anchor, controller);

        localPos = SlotLocalPos(stack.IndexOf(controller));

        return true;
    }

    public void Detach(PickupItemController controller)
    {
        if (!stack.Remove(controller))
            return;

        if (anchorProvider.TryGetAnchor(out var anchor))
            ReslotCarriedItems(anchor, null);
    }

    public void Clear()
    {
        stack.Clear();
    }

    private void EvictOldest()
    {
        var oldest = stack[0];

        stack.RemoveAt(0);
        Evicted?.Invoke(oldest);
    }

    private void ReslotCarriedItems(Transform anchor, PickupItemController skip)
    {
        for (var i = 0; i < stack.Count; i++)
        {
            var item = stack[i];

            if (item == skip)
                continue;

            item.MoveToCarrySlot(anchor, SlotLocalPos(i), Quaternion.identity);
        }
    }

    private Vector3 SlotLocalPos(int index)
    {
        return Vector3.up * (index * config.CarrySlotSpacing);
    }
}
