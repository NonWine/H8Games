using System;
using System.Collections.Generic;
using UnityEngine;

public class NullPickupCarrySink : IPickupCarrySink
{
    private static readonly IReadOnlyList<PickupItemController> EmptyCarried = Array.Empty<PickupItemController>();

    public event Action<PickupItemController> Evicted
    {
        add { }
        remove { }
    }

    public IReadOnlyList<PickupItemController> Carried => EmptyCarried;

    public bool TryAttach(PickupItemController controller, out Transform anchor, out Vector3 localPos, out Quaternion localRot)
    {
        anchor   = null;
        localPos = Vector3.zero;
        localRot = Quaternion.identity;

        return false;
    }

    public void Detach(PickupItemController controller) { }

    public bool TryDetachNewest(out PickupItemController controller)
    {
        controller = null;

        return false;
    }

    public void Clear() { }
}
