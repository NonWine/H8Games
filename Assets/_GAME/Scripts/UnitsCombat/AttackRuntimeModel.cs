public class AttackRuntimeModel
{
    public float Damage;
    public float Cooldown;
    public float CooldownRemaining;

    public AttackRuntimeModel(UnitStats stats)
    {
        Damage = stats.Damage;
        Cooldown = stats.AttackCooldown;
    }
}