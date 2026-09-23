using UnityEngine;
using Zenject;

public class CombatCameraRuntimePreview : MonoBehaviour
{
    [SerializeField] private bool forceCombatCamera;

    [Inject] private HeroCameraController cameraController;

    private bool appliedValue;
    private bool isApplied;

    private void Start()
    {
        Apply();
    }

    private void Update()
    {
        if (!isApplied || appliedValue != forceCombatCamera)
        {
            Apply();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && cameraController != null)
        {
            Apply();
        }
    }

    private void OnDisable()
    {
        if (cameraController != null)
        {
            cameraController.SetCombatPreview(false);
        }

        isApplied = false;
    }

    private void Apply()
    {
        appliedValue = forceCombatCamera;
        isApplied = true;
        cameraController.SetCombatPreview(forceCombatCamera);
    }
}
