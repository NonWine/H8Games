using UnityEngine;

[CreateAssetMenu(fileName = "PickupVisualConfig", menuName = "Configs/Pickup Visual Config")]
public class PickupVisualConfig : ScriptableObject
{
    [SerializeField] private PickupIdleSettings groundIdle = new PickupIdleSettings();
    public PickupIdleSettings GroundIdle => groundIdle;
    [SerializeField] private PickupSpendSettings spendPresentation = new PickupSpendSettings();
    public PickupSpendSettings SpendPresentation => spendPresentation;

    [Header("Scatter")]
    [SerializeField] private float minHorizSpeed = 1.25f;
    [SerializeField] private float maxHorizSpeed = 2.5f;
    [SerializeField] private float minVertSpeed = 1.75f;
    [SerializeField] private float maxVertSpeed = 3.25f;
    [SerializeField] private float maxAngularSpeed = 10f;
    [SerializeField] private bool  useGravity = true;

    // Unity's 3D Rigidbody has no gravityScale, so anything above 1 is applied
    // as extra downward acceleration while the pickup is in world physics.
    [SerializeField, Min(0f)] private float gravityMultiplier = 1f;

    [Header("Ground Impact FX")]
    [SerializeField] private LayerMask impactGroundMask = 1;
    [SerializeField, Min(0f)] private float impactMinSpeed = 2.5f;
    [SerializeField, Min(0f)] private float impactFxCooldown = 0.12f;

    [Header("Collect")]
    [SerializeField] private float collectDuration = 0.45f;
    [SerializeField] private float arcHeight = 1.5f;
    [SerializeField] private AnimationCurve collectCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float moveToSlotDuration = 0.15f;

    [Header("Spend")]
    [SerializeField] private float spendDuration = 0.35f;
    [SerializeField] private float jumpPower = 1.2f;
    [SerializeField] private float spendSpinSpeed = 540f;
    [SerializeField] private AnimationCurve spendCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0.3f, 0.3f), new Keyframe(1f, 1f, 1.8f, 1.8f));

    public float          MinHorizSpeed      => minHorizSpeed;
    public float          MaxHorizSpeed      => maxHorizSpeed;
    public float          MinVertSpeed       => minVertSpeed;
    public float          MaxVertSpeed       => maxVertSpeed;
    public float          MaxAngularSpeed    => maxAngularSpeed;
    public bool           UseGravity         => useGravity;
    public float          GravityMultiplier  => gravityMultiplier;
    public LayerMask      ImpactGroundMask   => impactGroundMask;
    public float          ImpactMinSpeed     => impactMinSpeed;
    public float          ImpactFxCooldown   => impactFxCooldown;
    public float          CollectDuration    => collectDuration;
    public float          ArcHeight          => arcHeight;
    public AnimationCurve CollectCurve       => collectCurve;
    public float          MoveToSlotDuration => moveToSlotDuration;
    public float          SpendDuration      => spendDuration;
    public float          JumpPower          => jumpPower;
    public float          SpendSpinSpeed     => spendSpinSpeed;
    public AnimationCurve SpendCurve         => spendCurve;
}
