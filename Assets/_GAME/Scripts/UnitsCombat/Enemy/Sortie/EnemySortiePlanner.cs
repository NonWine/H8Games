using UnityEngine;

public class EnemySortiePlanner
{
    private readonly EnemySortieConfig config;

    private bool isInFight;
    private bool willSortieThisFight;
    private int sortiesThisFight;
    private float nextSortieTime;

    public Vector3 Destination { get; private set; }

    public EnemySortiePlanner(EnemySortieConfig config)
    {
        this.config = config;
    }

    public void BeginFight(float now)
    {
        if (isInFight)
        {
            return;
        }

        isInFight = true;
        sortiesThisFight = 0;
        willSortieThisFight = Random.value < config.Chance;
        nextSortieTime = now + RandomIn(config.FirstSortieDelayRange);
    }

    public void EndFight()
    {
        isInFight = false;
        willSortieThisFight = false;
    }

    public bool TryPlanSortie(float now, Vector3 post, Vector3 position, Vector3 targetPosition)
    {
        if (!IsSortieDue(now))
        {
            return false;
        }

        sortiesThisFight++;
        nextSortieTime = now + RandomIn(config.RepeatSortieDelayRange);

        if (!TryPickDestination(post, position, targetPosition, out Vector3 destination))
        {
            return false;
        }

        Destination = destination;
        return true;
    }

    private bool IsSortieDue(float now)
    {
        return isInFight
               && willSortieThisFight
               && sortiesThisFight < config.MaxSortiesPerFight
               && now >= nextSortieTime;
    }

    private bool TryPickDestination(Vector3 post, Vector3 position, Vector3 targetPosition, out Vector3 destination)
    {
        destination = position;

        Vector3 toTarget = Flatten(targetPosition - position);
        float distanceToTarget = toTarget.magnitude;
        float step = Mathf.Min(
            RandomIn(config.StepDistanceRange),
            distanceToTarget - config.MinDistanceToTarget);

        if (step < config.MinStepDistance)
        {
            return false;
        }

        float yaw = Random.Range(-config.HeadingJitterAngle, config.HeadingJitterAngle);
        Vector3 heading = Quaternion.AngleAxis(yaw, Vector3.up) * (toTarget / distanceToTarget);
        Vector3 candidate = ClampToLeash(post, position + heading * step);
        candidate.y = position.y;

        if (Flatten(candidate - position).sqrMagnitude < config.MinStepDistance * config.MinStepDistance)
        {
            return false;
        }

        destination = candidate;
        return true;
    }

    private Vector3 ClampToLeash(Vector3 post, Vector3 candidate)
    {
        Vector3 fromPost = Flatten(candidate - post);

        if (fromPost.sqrMagnitude <= config.LeashRadius * config.LeashRadius)
        {
            return candidate;
        }

        return post + fromPost.normalized * config.LeashRadius;
    }

    private static float RandomIn(Vector2 range) => Random.Range(range.x, range.y);

    private static Vector3 Flatten(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
}
