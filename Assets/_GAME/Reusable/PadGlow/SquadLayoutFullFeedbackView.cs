using DG.Tweening;
using UnityEngine;

public class SquadLayoutFullFeedbackView : MonoBehaviour
{
    private static readonly int FillId = Shader.PropertyToID("_Fill");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int SizeId = Shader.PropertyToID("_Size");
    private static readonly int ShapeSizeId = Shader.PropertyToID("_ShapeSize");

    [SerializeField] private Renderer glowRenderer;
    [SerializeField] private RectTransform outlineTarget;
    [SerializeField] private Ease fillEase = Ease.InOutSine;
    [SerializeField, Min(0.01f)] private float fillDuration = 0.6f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.3f;

    [Header("Completion Flash")]
    [SerializeField, Min(1f)] private float flashIntensity = 1.8f;
    [SerializeField, Min(0.01f)] private float flashDuration = 0.3f;

    private MaterialPropertyBlock propertyBlock;
    private Sequence sequence;
    private float fill;
    private float intensity;
    private bool isFull;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        ApplyGeometry();
        ApplyInstant(false);
    }

    private void OnDestroy()
    {
        KillSequence();
    }

    public void SetFull(bool full, bool instant)
    {
        if (isFull == full)
        {
            return;
        }

        isFull = full;
        KillSequence();

        if (instant)
        {
            ApplyInstant(full);
            return;
        }

        sequence = full ? BuildFillSequence() : BuildFadeOutSequence();
    }

    private void ApplyInstant(bool full)
    {
        fill = full ? 1f : 0f;
        intensity = full ? 1f : 0f;
        Write();
        glowRenderer.enabled = full;
    }

    private Sequence BuildFillSequence()
    {
        fill = 0f;
        intensity = 1f;
        Write();
        glowRenderer.enabled = true;

        float flashHalf = flashDuration * 0.5f;

        return DOTween.Sequence()
            .Append(DOTween.To(() => fill, SetFill, 1f, fillDuration).SetEase(fillEase))
            .Append(DOTween.To(() => intensity, SetIntensity, flashIntensity, flashHalf).SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => intensity, SetIntensity, 1f, flashHalf).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    private Sequence BuildFadeOutSequence()
    {
        return DOTween.Sequence()
            .Append(DOTween.To(() => intensity, SetIntensity, 0f, fadeOutDuration).SetEase(Ease.InSine))
            .SetLink(gameObject)
            .OnComplete(() => ApplyInstant(false));
    }

    private void SetFill(float value)
    {
        fill = value;
        Write();
    }

    private void SetIntensity(float value)
    {
        intensity = value;
        Write();
    }

    private void ApplyGeometry()
    {
        Vector3 quadScale = glowRenderer.transform.lossyScale;
        Vector3[] corners = new Vector3[4];
        outlineTarget.GetWorldCorners(corners);
        float shapeWidth = Vector3.Distance(corners[0], corners[3]);
        float shapeDepth = Vector3.Distance(corners[0], corners[1]);

        glowRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector(SizeId, new Vector4(quadScale.x, quadScale.y, 0f, 0f));
        propertyBlock.SetVector(ShapeSizeId, new Vector4(shapeWidth, shapeDepth, 0f, 0f));
        glowRenderer.SetPropertyBlock(propertyBlock);
    }

    private void Write()
    {
        glowRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(FillId, fill);
        propertyBlock.SetFloat(IntensityId, intensity);
        glowRenderer.SetPropertyBlock(propertyBlock);
    }

    private void KillSequence()
    {
        sequence?.Kill();
        sequence = null;
    }
}
