using UnityEngine;

public interface IPickupCarryAnchorProvider
{
    bool TryGetAnchor(out Transform anchor);
}
