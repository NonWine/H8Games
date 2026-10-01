// Fired by LevelTransitionView after the screen overlay has fully faded out
// and the new level is visible. UI that wants to animate "welcome to level N"
// feedback should subscribe to this rather than LoadNextLevelSignal, which
// fires while the overlay is still opaque.
public class LevelTransitionCompletedSignal
{
}
