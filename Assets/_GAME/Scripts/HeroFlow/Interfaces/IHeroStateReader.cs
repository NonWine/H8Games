public interface IHeroStateReader
{
    HeroStats RuntimeStats { get; }
    bool IsAlive { get; }
}
