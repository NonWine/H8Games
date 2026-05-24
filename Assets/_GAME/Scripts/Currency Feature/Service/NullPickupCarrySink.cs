using UnityEngine;

public sealed class NullPickupCarrySink : IPickupCarrySink
{
    public bool TryAttach(PickupItemController controller, out Transform anchor, out Vector3 localPos, out Quaternion localRot)
    {
        anchor   = null;
        localPos = Vector3.zero;
        localRot = Quaternion.identity;

        return false;
    }

    public void Detach(PickupItemController controller) { }
}
