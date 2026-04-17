using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class ProjectileVisualSpawner
{
    private readonly SimpleProjectileView projectilePrefab;

    public ProjectileVisualSpawner(SimpleProjectileView projectilePrefab)
    {
        this.projectilePrefab = projectilePrefab;
    }

    public bool Spawn(Transform origin, Transform target, float speed, Action onHit)
    {
        if (target == null)
            return false;

        SimpleProjectileView projectile = Object.Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        projectile.Launch(target, speed, onHit);
        return true;
    }
}
