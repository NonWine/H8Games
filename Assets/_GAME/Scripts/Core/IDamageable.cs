using UnityEngine;

public interface IDamageable
{
    void GetDamage(float damage, Vector3 sourceWorldPosition);
    bool IsAlive { get; }
    Transform transform { get; }
}
