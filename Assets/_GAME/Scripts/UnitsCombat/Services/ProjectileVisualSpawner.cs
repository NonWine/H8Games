using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class ProjectileVisualSpawner
{
    private readonly SimpleProjectileView projectilePrefab;
    private readonly IAudioService audioService;

    public ProjectileVisualSpawner(SimpleProjectileView projectilePrefab, IAudioService audioService)
    {
        this.projectilePrefab = projectilePrefab;
        this.audioService = audioService;
    }

    public bool Spawn(Transform origin, Transform target, float speed, Action onHit)
    {
        if (target == null)
            return false;

        SimpleProjectileView projectile = Object.Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        projectile.Launch(target, speed, onHit);

        // Played here rather than off a signal so the shot lands on the same
        // frame as the projectile leaving the muzzle. The catalog entry is what
        // keeps a full squad volley from stacking into a click.
        audioService.Play(SfxId.Shoot);
        return true;
    }
}
