using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class CameraAnchorRotationStabilizer : MonoBehaviour
{
    private Quaternion worldRotation;

    private void Awake()
    {
        worldRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = worldRotation;
    }
}
