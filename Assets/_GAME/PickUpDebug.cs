using UnityEngine;
using Zenject;

public class PickUpDebug : MonoBehaviour
{
    [Inject] private IPickupService pickupService;
    [SerializeField] private Transform pickupTransform;
    
    [ContextMenu("Create Pickup")]
    public void CreateCoin()
    {
        pickupService.SpawnAsync(new PickupSpawnRequest("coin", 1, pickupTransform.transform.position));
    }
}
