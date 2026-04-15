using UnityEngine;
using Zenject;

public class SoldierFollower : MonoBehaviour
{

    private SquadFollowSettings settings;
    private ISquadSlotPositionProvider squadSlotPositionProvider;
    private ISoldierFollowerRegistratorProvider registrator;
    private SquadRoot squadRoot;
    private FormationSlot assignedSlot;

    public FormationSlot AssignedSlot => assignedSlot;

    [Inject]
    public void Construct(SquadFollowSettings settings, SquadRoot squadRoot)
    {
        this.settings = settings;
        this.squadRoot = squadRoot;
    }
    

    private void Update()
    {
        if (assignedSlot == null)
            return;

        Vector3 desiredPosition = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        desiredPosition.y = transform.position.y;

        Vector3 delta = desiredPosition - transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance <= settings.SlotReachThreshold)
        {
            transform.position = new Vector3(desiredPosition.x, transform.position.y, desiredPosition.z);
            RotateTowards(squadRoot.transform.forward, Time.deltaTime);
            return;
        }


        float slowdownRadius = Mathf.Max(settings.SlotReachThreshold * 4f, settings.SlotReachThreshold + 0.01f);
        float speedFactor = distance < slowdownRadius
            ? Mathf.Lerp(0.35f, 1f, distance / slowdownRadius)
            : 1f;

        float step = settings.SoldierMoveSpeed * speedFactor * Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, desiredPosition, step);
        transform.position = new Vector3(nextPosition.x, transform.position.y, nextPosition.z);
        RotateTowards(delta.normalized, Time.deltaTime);
    }

    private void OnDisable()
    {
        squadRoot = null;
        assignedSlot = null;
        registrator.UnregisterSoldier(this);
    }

    public void AssignSquad(SquadRoot squadRoot)
    {
        this.squadRoot = squadRoot;
    }

    public void AssignSlot(FormationSlot slot)
    {
        assignedSlot = slot;
    }

    public void ClearSquad(SquadRoot owner)
    {
        if (squadRoot != owner)
            return;

        assignedSlot = null;
        squadRoot = null;
    }

    public bool IsInAssignedSlot(float threshold)
    {
        if (assignedSlot == null)
            return false;

        Vector3 targetPosition = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        Vector3 delta = targetPosition - transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude <= threshold * threshold;
    }

    private void RotateTowards(Vector3 direction, float deltaTime)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            settings.SoldierRotationSpeed * deltaTime);
    }
}
