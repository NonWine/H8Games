using DG.Tweening;
using UnityEngine;

// Arrival feedback for a single formation slot marker. Presentation only: it is
// told that someone arrived and plays the punch/flash, it never asks who.
[DisallowMultipleComponent]
public class SquadSlotMarkerView : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Renderer markerRenderer;

    [Header("Punch")]
    [SerializeField, Min(0f)] private float punchScale = 0.45f;
    [SerializeField, Min(0.01f)] private float punchDuration = 0.3f;
    [SerializeField, Min(0)] private int punchVibrato = 7;
    [SerializeField, Range(0f, 1f)] private float punchElasticity = 0.7f;

    [Header("Flash")]
    [SerializeField] private Color flashColor = new(1f, 0.92f, 0.45f, 1f);
    [SerializeField, Min(0.01f)] private float flashDuration = 0.35f;

    [Header("Retrigger")]
    [SerializeField, Min(0f)] private float minInterval = 0.2f;

    private MaterialPropertyBlock propertyBlock;
    private Tween punchTween;
    private Tween flashTween;
    private Vector3 baseScale;
    private Color baseColor;
    private float nextAllowedTime;

    // Move and Idle share one slot-reach threshold with no hysteresis, so a
    // soldier settling right on it can flip between them for a few frames. The
    // gate keeps that from reading as a stuck, permanently lit marker.
    public void PlayArrival()
    {
        if (Time.time < nextAllowedTime)
        {
            return;
        }

        nextAllowedTime = Time.time + minInterval;
        PlayPunch();
        PlayFlash();
    }

    private void Awake()
    {
        baseScale = transform.localScale;
        propertyBlock = new MaterialPropertyBlock();
        baseColor = markerRenderer.sharedMaterial.GetColor(BaseColorId);
    }

    // Tweens are linked to the GameObject, which covers destruction but not the
    // deactivation ApplyLayout does when the squad shrinks - a marker parked
    // mid-punch would come back scaled and tinted.
    private void OnDisable()
    {
        punchTween?.Kill();
        flashTween?.Kill();
        punchTween = null;
        flashTween = null;
        nextAllowedTime = 0f;
        transform.localScale = baseScale;
        ApplyColor(baseColor);
    }

    private void PlayPunch()
    {
        punchTween?.Kill();
        transform.localScale = baseScale;
        punchTween = transform
            .DOPunchScale(baseScale * punchScale, punchDuration, punchVibrato, punchElasticity)
            .SetLink(gameObject)
            .OnComplete(() => transform.localScale = baseScale);
    }

    private void PlayFlash()
    {
        flashTween?.Kill();
        ApplyColor(flashColor);
        flashTween = DOVirtual
            .Float(0f, 1f, flashDuration, progress => ApplyColor(Color.Lerp(flashColor, baseColor, progress)))
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void ApplyColor(Color color)
    {
        markerRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, color);
        markerRenderer.SetPropertyBlock(propertyBlock);
    }
}
