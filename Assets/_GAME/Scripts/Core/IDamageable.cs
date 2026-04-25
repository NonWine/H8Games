using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Vector3 sourceWorldPosition);
    bool IsAlive { get; }
    Transform transform { get; }
}
