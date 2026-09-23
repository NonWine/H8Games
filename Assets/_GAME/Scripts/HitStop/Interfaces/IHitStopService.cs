// Narrow contract between "something landed hard" and "time crawls for a
// moment". Callers never touch Time.timeScale, which is what keeps the restore
// in exactly one place.
public interface IHitStopService
{
    // A request arriving during an active freeze replaces it rather than
    // stacking, so overlapping kills can never multiply into slow motion.
    void Request(float timeScale, float durationSeconds);
}
