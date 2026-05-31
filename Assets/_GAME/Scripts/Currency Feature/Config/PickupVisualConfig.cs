using UnityEngine;

[CreateAssetMenu(fileName = "PickupVisualConfig", menuName = "Configs/Pickup Visual Config")]
public sealed class PickupVisualConfig : ScriptableObject
{
    [Header("Scatter")]
    [SerializeField] private float minHorizSpeed   = 1.25f;
    [SerializeField] private float maxHorizSpeed   = 2.5f;
    [SerializeField] private float minVertSpeed    = 1.75f;
    [SerializeField] private float maxVertSpeed    = 3.25f;
    [SerializeField] private float maxAngularSpeed = 10f;
    [SerializeField] private bool  useGravity      = true;

    [Header("Settle")]
    [SerializeField] private float settleDelay = 0.7f;

    [Header("Collect Arc")]
    [SerializeField] private float          collectDuration = 0.45f;
    [SerializeField] private float          arcHeight       = 1.5f;
    [SerializeField] private AnimationCurve collectCurve   = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Slot Move")]
    [SerializeField] private float moveToSlotDuration = 0.15f;

    [Header("Spend")]
    [SerializeField] private float          spendDuration = 0.35f;
    [SerializeField] private float          jumpPower     = 1.2f;
    [SerializeField] private AnimationCurve spendCurve   = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float          MinHorizSpeed      => minHorizSpeed;
    public float          MaxHorizSpeed      => maxHorizSpeed;
    public float          MinVertSpeed       => minVertSpeed;
    public float          MaxVertSpeed       => maxVertSpeed;
    public float          MaxAngularSpeed    => maxAngularSpeed;
    public bool           UseGravity         => useGravity;
    public float          SettleDelay        => settleDelay;
    public float          CollectDuration    => collectDuration;
    public float          ArcHeight          => arcHeight;
    public AnimationCurve CollectCurve       => collectCurve;
    public float          MoveToSlotDuration => moveToSlotDuration;
    public float          SpendDuration      => spendDuration;
    public float          JumpPower          => jumpPower;
    public AnimationCurve SpendCurve         => spendCurve;
}
