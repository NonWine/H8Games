using UnityEngine;
using Zenject;

[RequireComponent(typeof(Collider))]
public sealed class EnemyEncounterZone : MonoBehaviour
{
    [SerializeField] private EnemyGroupFacade enemyGroup;

    private SquadFlowCoordinator squadFlowCoordinator;
    public EnemyGroupFacade EnemyGroup => enemyGroup;

    [Inject]
    public void Construct(SquadFlowCoordinator squadFlowCoordinator)
    {
        this.squadFlowCoordinator = squadFlowCoordinator;
    }

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
            trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        NotifyIfSquadRoot(other);
    }
    

    private void NotifyIfSquadRoot(Collider other)
    {
        
        if (other.GetComponentInParent<SquadRoot>() == null)
            return;

        squadFlowCoordinator.NotifyEncounterZoneEntered(enemyGroup);
    }
}
