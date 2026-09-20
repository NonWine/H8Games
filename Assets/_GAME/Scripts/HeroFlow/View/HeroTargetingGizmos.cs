using UnityEngine;
using Zenject;

// Draws nothing but the hero's sensing state: the configured detection radius
// and a line to whatever the tracker is holding right now. It owns no targeting
// logic and nothing depends on it, so deleting the component removes the
// visualisation and changes no behaviour.
public class HeroTargetingGizmos : MonoBehaviour
{
    private const float VerticalLineOffset = 1f;

    [SerializeField] private Color radiusColor = new Color(0.25f, 0.8f, 1f, 0.75f);
    [SerializeField] private Color targetLineColor = new Color(1f, 0.3f, 0.2f, 1f);
    [SerializeField, Min(0f)] private float targetMarkerRadius = 0.4f;

    private TargetingData targetingData;
    private ITargetTrackerHandler targetTracker;

    [Inject]
    public void Construct(TargetingData targetingData, ITargetTrackerHandler targetTracker)
    {
        this.targetingData = targetingData;
        this.targetTracker = targetTracker;
    }

    // Injection only happens once the GameObjectContext has run, so both
    // dependencies are null while the prefab sits in the project view.
    private void OnDrawGizmos()
    {
        if (targetingData == null)
        {
            return;
        }

        DrawDetectionRadius();
        DrawCurrentTargetLink();
    }

    private void DrawDetectionRadius()
    {
        Gizmos.color = radiusColor;
        Gizmos.DrawWireSphere(transform.position, targetingData.DetectionRadius);
    }

    private void DrawCurrentTargetLink()
    {
        ICombatTarget target = targetTracker.CurrentTarget;
        if (target == null || !target.IsAlive)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * VerticalLineOffset;
        Vector3 targetPosition = target.transform.position + Vector3.up * VerticalLineOffset;

        Gizmos.color = targetLineColor;
        Gizmos.DrawLine(origin, targetPosition);
        Gizmos.DrawWireSphere(targetPosition, targetMarkerRadius);
    }
}
