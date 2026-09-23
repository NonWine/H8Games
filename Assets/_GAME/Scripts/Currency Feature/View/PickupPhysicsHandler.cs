using System.Collections.Generic;
using UnityEngine;

public class PickupPhysicsHandler
{
    private readonly Transform             transform;
    private readonly Rigidbody             rb;
    private readonly Collider[]            colliders;
    private readonly bool                  defaultUseGravity;
    private readonly bool                  defaultIsKinematic;
    private readonly bool                  defaultDetectCollisions;
    private readonly RigidbodyInterpolation defaultInterpolation;
    private readonly CollisionDetectionMode defaultCollisionMode;

    private float gravityMultiplier = 1f;
    private bool  isWorldGravityActive;
    private readonly Dictionary<Collider, float> supportContacts = new Dictionary<Collider, float>();

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

    public void TrackSupport(Collision collision)
    {
        float normal = -1f;
        for (int i = 0; i < collision.contactCount; i++)
            normal = Mathf.Max(normal, Vector3.Dot(collision.GetContact(i).normal, Vector3.up));
        supportContacts[collision.collider] = normal;
    }

    public void RemoveSupport(Collision collision)
    {
        supportContacts.Remove(collision.collider);
    }

    public bool IsSettled(PickupIdleSettings settings)
    {
        if (rb.isKinematic || !rb.detectCollisions ||
            rb.linearVelocity.sqrMagnitude > settings.MaxLinearSpeed * settings.MaxLinearSpeed ||
            rb.angularVelocity.sqrMagnitude > settings.MaxAngularSpeed * settings.MaxAngularSpeed)
            return false;

        foreach (KeyValuePair<Collider, float> contact in supportContacts)
        {
            if (contact.Key != null && contact.Key.enabled && contact.Key.gameObject.activeInHierarchy &&
                (settings.SupportMask.value & (1 << contact.Key.gameObject.layer)) != 0 &&
                contact.Value >= settings.MinSupportNormal)
                return true;
        }
        return false;
    }

    public void PlaceAt(Vector3 position, Quaternion rotation)
    {
        supportContacts.Clear();
        rb.isKinematic = true;
        rb.position    = position;
        rb.rotation    = rotation;
        transform.SetPositionAndRotation(position, rotation);
    }

    public void EnableWorldPhysics(bool useGravity, float gravityMultiplier)
    {
        supportContacts.Clear();
        EnableColliders();

        rb.useGravity             = useGravity;
        rb.isKinematic            = false;
        rb.detectCollisions       = true;
        rb.interpolation          = defaultInterpolation;
        rb.collisionDetectionMode = defaultCollisionMode;
        rb.WakeUp();

        this.gravityMultiplier = gravityMultiplier;
        isWorldGravityActive   = useGravity;
    }

    // Called every physics step while the pickup flies: Unity applies 1g on its
    // own, so only the surplus above it is added here.
    public void ApplyExtraGravity()
    {
        if (!isWorldGravityActive)
            return;

        if (Mathf.Approximately(gravityMultiplier, 1f))
            return;

        // AddForce wakes a sleeping body, so a settled pickup would never be
        // allowed to sleep again while this runs every physics step.
        if (rb.IsSleeping())
            return;

        rb.AddForce(UnityEngine.Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
    }

    public void EnableCarryPhysics()
    {
        supportContacts.Clear();
        DisableColliders();

        rb.useGravity             = false;
        rb.isKinematic            = true;
        rb.detectCollisions       = false;
        rb.interpolation          = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.Sleep();

        isWorldGravityActive = false;
    }

    public void RestOnGround()
    {
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic     = true;
        rb.Sleep();

        isWorldGravityActive = false;
    }

    public void RestoreDefaults()
    {
        supportContacts.Clear();
        EnableColliders();

        rb.useGravity             = defaultUseGravity;
        rb.isKinematic            = defaultIsKinematic;
        rb.detectCollisions       = defaultDetectCollisions;
        rb.interpolation          = defaultInterpolation;
        rb.collisionDetectionMode = defaultCollisionMode;

        gravityMultiplier    = 1f;
        isWorldGravityActive = false;
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
