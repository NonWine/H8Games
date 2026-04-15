using UnityEngine;

public class SquadHomeController
{
    private readonly Transform rootTransform;

    public Vector3 HomePosition { get; }
    public Quaternion HomeRotation { get; }

    public SquadHomeController(Transform rootTransform)
    {
        this.rootTransform = rootTransform;
        HomePosition = rootTransform.position;
        HomeRotation = rootTransform.rotation;
    }

    public void SnapToHome()
    {
        rootTransform.position = HomePosition;
        rootTransform.rotation = HomeRotation;
    }
}