// The game's sound vocabulary. Deliberately a flat enum rather than string keys:
// the catalog is authored by hand for a single slice, so a typo should be a
// compile error, not a silent miss at runtime.
public enum SfxId
{
    None = 0,

    BattleStart = 1,
    SquadClash = 2,
    UnitSpawn = 3,

    Shoot = 10,
    Hit = 11,
    Kill = 12,
    TargetSwitch = 13,

    HeroHurt = 20,
    HeroDown = 21,

    CoinPickup = 30,
    CoinDeposit = 31,
    UpgradePurchase = 32,
    BarracksBuild = 33,
    BarracksPropLand = 34,

    LevelWin = 40,
    CaptureComplete = 41,
    CaptureTick = 42,
}
