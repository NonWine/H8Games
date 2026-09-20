using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPickupCarrySink
{
    event Action<PickupItemController> Evicted;

    IReadOnlyList<PickupItemController> Carried { get; }

    bool TryAttach(PickupItemController controller, out Transform anchor, out Vector3 localPos, out Quaternion localRot);
    void Detach(PickupItemController controller);
    bool TryDetachNewest(out PickupItemController controller);
    void Clear();
}
