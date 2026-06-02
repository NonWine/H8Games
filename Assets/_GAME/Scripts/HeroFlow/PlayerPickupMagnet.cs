using UnityEngine;

public class PlayerPickupMagnet : MonoBehaviour, IPickupMagnetProvider
{
    [SerializeField] private float radius = 3f;

    public bool TryGetMagnet(out PickupMagnet magnet)
    {
        magnet = new PickupMagnet(transform.position, radius, transform);
        return true;
    }
}
