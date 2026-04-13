using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBarView : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform canvasRoot;

    public void SetHealth(float current, float max)
    {
        if (slider == null)
            return;

        slider.maxValue = max;
        slider.value = current;
    }

    private void LateUpdate()
    {
        if (canvasRoot == null || Camera.main == null)
            return;

        canvasRoot.forward = Camera.main.transform.forward;
    }
}
