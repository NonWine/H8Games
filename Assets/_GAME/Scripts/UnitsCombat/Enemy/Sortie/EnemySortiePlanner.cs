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

    private static readonly Collider[] OverlapBuffer = new Collider[16];

    private bool IsSortieDue(float now)
    {
        return isInFight
               && willSortieThisFight
               && sortiesThisFight < config.MaxSortiesPerFight
               && now >= nextSortieTime
               && EnemySortieCoordinator.CanSortie(config.MaxConcurrentSorties);
    }

    private bool TryPickDestination(Vector3 post, Vector3 position, Vector3 targetPosition, out Vector3 destination)
    {
        destination = position;

        Vector3 toTarget = Flatten(targetPosition - position);
        float distanceToTarget = toTarget.magnitude;
        if (distanceToTarget < 0.001f)
        {
            return false;
        }

        Vector3 dirToTarget = toTarget / distanceToTarget;

        // Randomized stopping distance per unit so enemies form a layered firing arc rather than bunching
        float preferredStopDistance = config.MinDistanceToTarget + Random.Range(0f, 1.4f);
        float maxStepAllowed = Mathf.Max(0f, distanceToTarget - preferredStopDistance);
        float desiredStep = RandomIn(config.StepDistanceRange);
        float step = Mathf.Min(desiredStep, maxStepAllowed);

        if (step < config.MinStepDistance)
        {
            return false;
        }

        // Generate multiple candidate flanking angles to find a destination with good clearance from teammates
        float maxJitter = config.HeadingJitterAngle;
        float[] candidateAngles = new float[]
        {
            Random.Range(-maxJitter, maxJitter),
            Random.Range(20f, maxJitter),
            Random.Range(-maxJitter, -20f),
            0f
        };

        Vector3 bestCandidate = Vector3.zero;
        bool foundCandidate = false;

        for (int i = 0; i < candidateAngles.Length; i++)
        {
            float yaw = candidateAngles[i];
            Vector3 heading = Quaternion.AngleAxis(yaw, Vector3.up) * dirToTarget;
            Vector3 candidate = ClampToLeash(post, position + heading * step);
            candidate.y = position.y;

            if (Flatten(candidate - position).sqrMagnitude < config.MinStepDistance * config.MinStepDistance)
            {
                continue;
            }

            // Check that this candidate does NOT bunch up with another enemy or active sortie destination
            if (HasTeammateClearance(candidate, position, config.MinAlliedSeparation))
            {
                bestCandidate = candidate;
                foundCandidate = true;
                break;
            }

            if (!foundCandidate)
            {
                bestCandidate = candidate;
                foundCandidate = true;
            }
        }

        if (!foundCandidate)
        {
            return false;
        }

        destination = bestCandidate;
        return true;
    }

    private static bool HasTeammateClearance(Vector3 candidate, Vector3 selfPosition, float minSeparation)
    {
        // 1. Check against other enemies' active sortie destinations
        if (!EnemySortieCoordinator.IsPositionClear(candidate, minSeparation))
        {
            return false;
        }

        // 2. Check against other physical agents on the battlefield (layer 7 = Agent)
        int count = Physics.OverlapSphereNonAlloc(candidate, minSeparation, OverlapBuffer, 1 << 7);
        for (int i = 0; i < count; i++)
        {
            Collider col = OverlapBuffer[i];
            if (col == null) continue;

            Vector3 otherPos = col.transform.position;
            otherPos.y = 0f;
            Vector3 selfPos = selfPosition;
            selfPos.y = 0f;

            // Ignore own body parts
            if (Vector3.Distance(otherPos, selfPos) < 0.4f)
            {
                continue;
            }

            return false;
        }

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
