/// <summary>
/// Stable value-noise helper used to give squad members individual, repeatable
/// variation without touching the shared random sequence.
/// </summary>
public static class DeterministicHashUtility
{
    private const uint HashNoise = 2747636419u;
    private const uint HashMultiplier = 2654435769u;
    private const uint MantissaMask = 0x00FFFFFFu;
    private const float MantissaMaxValue = 16777215f;
    private const int ShiftBits = 16;

    public static float Hash01(int value)
    {
        unchecked
        {
            uint x = (uint)(value < 0 ? -value : value) + 1u;
            x ^= HashNoise;
            x *= HashMultiplier;
            x ^= x >> ShiftBits;
            x *= HashMultiplier;
            x ^= x >> ShiftBits;
            x *= HashMultiplier;

            return (x & MantissaMask) / MantissaMaxValue;
        }
    }
}
