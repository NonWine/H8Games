public class UnitRuntimeConfig
{
    public UnitStats Stats { get; }

    public UnitRuntimeConfig(UnitStats sourceStats)
    {
        Stats = new UnitStats(sourceStats);
    }
}