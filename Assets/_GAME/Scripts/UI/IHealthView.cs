// Narrow contract between the combat model and whatever draws health: the
// presenter only ever pushes a value, so nothing on the model side needs to
// know a Slider, a Canvas or DOTween exists.
public interface IHealthView
{
    void SetHealth(float current, float max);
}
