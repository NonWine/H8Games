using UnityEngine;

public class SoldierMovingFormationService
{
    private readonly SquadFollowSettings settings;
    private readonly Vector2 movingLocalOffset;
    private readonly float moveSpeedMultiplier;
    private readonly float rotationSpeedMultiplier;
    private readonly float movingYawOffset;

    private Vector3 followVelocity;

    public SoldierMovingFormationService(SquadFollowSettings settings, int seed)
    {
        this.settings = settings;

        moveSpeedMultiplier = Mathf.Lerp(
            settings.SoldierMoveSpeedMultiplierMin,
            settings.SoldierMoveSpeedMultiplierMax,
            Hash01(seed * 17 + 3));

        rotationSpeedMultiplier = Mathf.Lerp(
            settings.SoldierRotationSpeedMultiplierMin,
            settings.SoldierRotationSpeedMultiplierMax,
            Hash01(seed * 31 + 7));

        float offsetX = Mathf.Lerp(
            -settings.MovingSlotOffsetRadius,
            settings.MovingSlotOffsetRadius,
            Hash01(seed * 47 + 11));

        float offsetZ = Mathf.Lerp(
            -settings.MovingSlotOffsetRadius,
            settings.MovingSlotOffsetRadius,
            Hash01(seed * 59 + 13));

        movingLocalOffset = new Vector2(offsetX, offsetZ);
        movingYawOffset = Mathf.Lerp(
            -settings.MovingFacingYawJitter,
            settings.MovingFacingYawJitter,
            Hash01(seed * 71 + 17));
    }

    public SoldierFormationState Update(Transform soldierTransform, Transform squadRoot, Vector3 slotCenter, float deltaTime)
    {
        Vector3 desiredPosition = slotCenter + GetMovingWorldOffset(squadRoot);
        desiredPosition.y = soldierTransform.position.y;

        Vector3 delta = desiredPosition - soldierTransform.position;
        delta.y = 0f;

        if (delta.magnitude <= settings.SlotReachThreshold)
        {
            RotateTowards(
                soldierTransform,
                GetMovingLookDirection(squadRoot, delta),
                deltaTime,
                settings.SoldierRotationSpeed * rotationSpeedMultiplier);

            return SoldierFormationState.WaitingInFormation;
        }

        Vector3 nextPosition = Vector3.SmoothDamp(
            soldierTransform.position,
            desiredPosition,
            ref followVelocity,
            settings.SoldierFollowSmoothTime,
            settings.SoldierMoveSpeed * moveSpeedMultiplier,
            deltaTime);

        soldierTransform.position = new Vector3(nextPosition.x, soldierTransform.position.y, nextPosition.z);
        RotateTowards(
            soldierTransform,
            GetMovingLookDirection(squadRoot, delta),
            deltaTime,
            settings.SoldierRotationSpeed * rotationSpeedMultiplier);

        return SoldierFormationState.MovingToSlot;
    }

    public void Reset()
    {
        followVelocity = Vector3.zero;
    }

    private Vector3 GetMovingWorldOffset(Transform squadRoot)
    {
        return squadRoot.right * movingLocalOffset.x
               + squadRoot.forward * movingLocalOffset.y;
    }

    private Vector3 GetMovingLookDirection(Transform squadRoot, Vector3 toSlot)
    {
        Vector3 rootForward = squadRoot.forward;
        Vector3 slotDirection = toSlot.sqrMagnitude > 0.0001f ? toSlot.normalized : rootForward;
        Vector3 blendedDirection = Vector3.Lerp(rootForward, slotDirection, settings.MovingFacingToSlotWeight).normalized;
        return Quaternion.AngleAxis(movingYawOffset, Vector3.up) * blendedDirection;
    }

    private static void RotateTowards(Transform soldierTransform, Vector3 direction, float deltaTime, float rotationSpeed)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        soldierTransform.rotation = Quaternion.RotateTowards(
            soldierTransform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);
    }

    private static float Hash01(int value)
    {
        unchecked
        {
            uint x = (uint)(Mathf.Abs(value) + 1);
            x ^= 2747636419u;
            x *= 2654435769u;
            x ^= x >> 16;
            x *= 2654435769u;
            x ^= x >> 16;
            x *= 2654435769u;
            return (x & 0x00FFFFFFu) / 16777215f;
        }
    }
}
