using UnityEngine;

public sealed class PickupPhysicsHandler
{
    private readonly Transform             transform;
    private readonly Rigidbody             rb;
    private readonly Collider[]            colliders;
    private readonly bool                  defaultUseGravity;
    private readonly bool                  defaultIsKinematic;
    private readonly bool                  defaultDetectCollisions;
    private readonly RigidbodyInterpolation defaultInterpolation;
    private readonly CollisionDetectionMode defaultCollisionMode;

    public PickupPhysicsHandler(Transform transform, Rigidbody rb, Collider[] colliders)
    {
        this.transform = transform;
        this.rb        = rb;
        this.colliders = colliders;

        defaultUseGravity       = rb.useGravity;
        defaultIsKinematic      = rb.isKinematic;
        defaultDetectCollisions = rb.detectCollisions;
        defaultInterpolation    = rb.interpolation;
        defaultCollisionMode    = rb.collisionDetectionMode;
    }

    public void PlaceAt(Vector3 position, Quaternion rotation)
    {
        rb.isKinematic = true;
        rb.position    = position;
        rb.rotation    = rotation;
        transform.SetPositionAndRotation(position, rotation);
    }

    public void EnableWorldPhysics(bool useGravity)
    {
        EnableColliders();

        rb.useGravity             = useGravity;
        rb.isKinematic            = false;
        rb.detectCollisions       = true;
        rb.interpolation          = defaultInterpolation;
        rb.collisionDetectionMode = defaultCollisionMode;
        rb.WakeUp();
    }

    public void EnableCarryPhysics()
    {
        DisableColliders();

        rb.useGravity             = false;
        rb.isKinematic            = true;
        rb.detectCollisions       = false;
        rb.interpolation          = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.Sleep();
    }

    public void RestoreDefaults()
    {
        EnableColliders();

        rb.useGravity             = defaultUseGravity;
        rb.isKinematic            = defaultIsKinematic;
        rb.detectCollisions       = defaultDetectCollisions;
        rb.interpolation          = defaultInterpolation;
        rb.collisionDetectionMode = defaultCollisionMode;
    }

    public void ApplyScatterVelocity(Vector3 direction, float minHoriz, float maxHoriz, float minVert, float maxVert, float maxAngular)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            direction = Vector3.forward;
        else
            direction.Normalize();

        rb.linearVelocity  = direction * Random.Range(minHoriz, maxHoriz) + Vector3.up * Random.Range(minVert, maxVert);
        rb.angularVelocity = Random.insideUnitSphere * maxAngular;
    }

    private void EnableColliders()
    {
        for (var i = 0; i < colliders.Length; i++)
            colliders[i].enabled = true;
    }

    private void DisableColliders()
    {
        for (var i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }
}
