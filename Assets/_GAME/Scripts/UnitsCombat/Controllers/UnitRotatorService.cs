using UnityEngine;

public class UnitRotatorService
{
    private const float CombatRotationSmoothness = 12f;
    private const float MinRotationDirectionSqrMagnitude = 0.0001f;

    public void RotateTowards(Transform self, Transform target)
    {
        Vector3 direction = target.position - self.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= MinRotationDirectionSqrMagnitude)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float lerpFactor = 1f - Mathf.Exp(-CombatRotationSmoothness * Time.deltaTime);

        self.rotation = Quaternion.Slerp(self.rotation, targetRotation, lerpFactor);
    }
}
