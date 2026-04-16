using UnityEngine;

public class ProjectileVisualSpawner
{
    private readonly SimpleProjectileView projectilePrefab;

    public ProjectileVisualSpawner(SimpleProjectileView projectilePrefab)
    {
        this.projectilePrefab = projectilePrefab;
    }

    public void Spawn(Transform origin, Transform target, float speed)
    {
        if (projectilePrefab == null || origin == null || target == null)
            return;

        SimpleProjectileView projectile = Object.Instantiate(
            projectilePrefab,
            origin.position,
            Quaternion.identity);

        projectile.Launch(target, speed);
    }
}