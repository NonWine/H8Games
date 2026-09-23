using UnityEngine;

public class PickupImpactFxHandler
{
    private readonly ParticleSystem impactFx;

    private LayerMask groundMask;
    private float     minImpactSpeed;
    private float     cooldown;
    private float     nextAllowedTime;
    private bool      isEnabled;

    public PickupImpactFxHandler(ParticleSystem impactFx)
    {
        this.impactFx = impactFx;
    }

    public void Configure(LayerMask groundMask, float minImpactSpeed, float cooldown)
    {
        this.groundMask     = groundMask;
        this.minImpactSpeed = minImpactSpeed;
        this.cooldown       = cooldown;

        isEnabled       = true;
        nextAllowedTime = 0f;
    }

    public void Disable()
    {
        isEnabled = false;
    }

    public void HandleCollision(Collision collision)
    {
        if (!isEnabled)
            return;

        // Sparks are opt-in per pickup prefab: a pickup without an assigned
        // particle system simply lands silently.
        if (impactFx == null)
            return;

        if ((groundMask.value & (1 << collision.gameObject.layer)) == 0)
            return;

        if (collision.relativeVelocity.magnitude < minImpactSpeed)
            return;

        if (Time.time < nextAllowedTime)
            return;

        nextAllowedTime = Time.time + cooldown;

        var contact = collision.GetContact(0);

        impactFx.transform.SetPositionAndRotation(contact.point, Quaternion.LookRotation(contact.normal));
        impactFx.Play(true);
    }
}
