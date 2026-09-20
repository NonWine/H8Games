using UnityEngine;
using UnityEngine.UI;
using Zenject;

// Top-of-screen bar showing how much of the current level's enemies are dead.
// Purely reactive: it forwards LevelProgressTracker.Progress and does not
// decide when progress changes. The slider eases toward each new value instead
// of snapping, so a kill reads as a small, visible tick forward rather than a
// jump that is easy to miss.
public class LevelProgressView : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField, Min(0.01f)] private float fillSpeed = 1.5f;

    private LevelProgressTracker progressTracker;
    private float targetProgress;

    [Inject]
    public void Construct(LevelProgressTracker progressTracker)
    {
        this.progressTracker = progressTracker;
        this.progressTracker.ProgressChanged += SetTargetProgress;

        targetProgress = this.progressTracker.Progress;

        if (slider != null)
        {
            slider.value = targetProgress;
        }
    }

    private void OnDestroy()
    {
        if (progressTracker != null)
        {
            progressTracker.ProgressChanged -= SetTargetProgress;
        }
    }

    private void Update()
    {
        if (slider == null)
        {
            return;
        }

        slider.value = Mathf.MoveTowards(slider.value, targetProgress, fillSpeed * Time.deltaTime);
    }

    private void SetTargetProgress(float progress)
    {
        targetProgress = progress;
    }
}
