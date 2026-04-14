public interface ICombatTarget : IDamageable
{
    TeamId TeamId { get; }
    public int CurrentWeight {get; set; }
}
