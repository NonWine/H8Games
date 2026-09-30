using UnityEngine;

public readonly struct CameraFocusFrame
{
    public readonly Vector3 LookAtPoint;
    public readonly float DistanceScale;
    public readonly float OrbitDegrees;
    public readonly float FieldOfView;

    public CameraFocusFrame(Vector3 lookAtPoint, float distanceScale, float orbitDegrees, float fieldOfView)
    {
        LookAtPoint = lookAtPoint;
        DistanceScale = distanceScale;
        OrbitDegrees = orbitDegrees;
        FieldOfView = fieldOfView;
    }
}

public interface ICameraFocusState
{
    float Weight { get; }

    bool TryGetFrame(out CameraFocusFrame frame);
}
