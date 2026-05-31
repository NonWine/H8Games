using System;
using UnityEngine;

public sealed class PickupItemController
{
    private enum PickupState
    {
        None       = 0,
        World      = 1,
        Collecting = 2,
        Carried    = 3,
        Spending   = 4
    }

    private static readonly AnimationCurve DefaultCollectCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private static readonly AnimationCurve DefaultSpendCurve   = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private readonly PickupItemView view;

    private PickupVisualConfig visualConfig;
    private PickupState        state;
    private float              worldAge;
    private bool               isSettled;

    public string PickupId      { get; private set; }
    public int    Amount        { get; private set; }
    public bool   IsRented      => view.IsRented;
    public bool   IsWorldPickup => state == PickupState.World;
    public bool   IsCollecting  => state == PickupState.Collecting;
    public bool   IsCarried     => state == PickupState.Carried;
    public bool   IsSpending    => state == PickupState.Spending;

    public PickupItemView View => view;

    private float          CollectDuration    => visualConfig != null ? visualConfig.CollectDuration    : 0.45f;
    private float          ArcHeight          => visualConfig != null ? visualConfig.ArcHeight          : 1.5f;
    private AnimationCurve CollectCurve       => visualConfig != null ? visualConfig.CollectCurve       : DefaultCollectCurve;
    private float          MoveToSlotDuration => visualConfig != null ? visualConfig.MoveToSlotDuration : 0.15f;
    private float          SpendDuration      => visualConfig != null ? visualConfig.SpendDuration      : 0.35f;
    private float          JumpPower          => visualConfig != null ? visualConfig.JumpPower          : 1.2f;
    private float          SpendSpinSpeed     => visualConfig != null ? visualConfig.SpendSpinSpeed     : 540f;
    private AnimationCurve SpendCurve         => visualConfig != null ? visualConfig.SpendCurve         : DefaultSpendCurve;
    private bool           UseGravity         => visualConfig != null && visualConfig.UseGravity;
    private float          MinHorizSpeed      => visualConfig != null ? visualConfig.MinHorizSpeed      : 1.25f;
    private float          MaxHorizSpeed      => visualConfig != null ? visualConfig.MaxHorizSpeed      : 2.5f;
    private float          MinVertSpeed       => visualConfig != null ? visualConfig.MinVertSpeed       : 1.75f;
    private float          MaxVertSpeed       => visualConfig != null ? visualConfig.MaxVertSpeed       : 3.25f;
    private float          MaxAngularSpeed    => visualConfig != null ? visualConfig.MaxAngularSpeed    : 10f;
    private float          SettleDelay        => visualConfig != null ? visualConfig.SettleDelay        : 0.7f;

    public PickupItemController(PickupItemView view)
    {
        this.view = view;
    }

    public void SetVisualConfig(PickupVisualConfig config)
    {
        visualConfig = config;
    }

    public void InitializeAsWorldItem(string pickupId, int amount, Vector3 position, Vector3 scatterDirection)
    {
        PickupId  = pickupId;
        Amount    = amount;
        state     = PickupState.World;
        worldAge  = 0f;
        isSettled = false;

        view.SetActivePose(null);
        view.Transform.SetParent(null, true);
        view.Physics.PlaceAt(position, Quaternion.identity);
        view.Physics.EnableWorldPhysics(UseGravity);
        view.Physics.ApplyScatterVelocity(scatterDirection, MinHorizSpeed, MaxHorizSpeed, MinVertSpeed, MaxVertSpeed, MaxAngularSpeed);
    }

    public void PlayCollectAnimation(Transform anchor, Vector3 localTargetPos, Quaternion localTargetRot, Action onCompleted)
    {
        state = PickupState.Collecting;

        view.Animation.BeginCollect(anchor, localTargetPos, localTargetRot, onCompleted);
        view.Transform.SetParent(null, true);
        view.Physics.EnableCarryPhysics();
        view.SetActivePose(() => view.Animation.ApplyCollectPose(CollectDuration, ArcHeight, CollectCurve));
    }

    public void ForceAttachToCarry(Transform anchor, Vector3 localPos, Quaternion localRot)
    {
        state = PickupState.Carried;

        view.Animation.BeginCarry(anchor, localPos, localRot);
        view.Transform.SetParent(null, true);
        view.Physics.EnableCarryPhysics();
        view.SetActivePose(() => view.Animation.ApplyCarryPose());
        view.Animation.ApplyCarryPose();
    }

    public void MoveToCarrySlot(Transform anchor, Vector3 localPos, Quaternion localRot)
    {
        if (state != PickupState.Carried)
        {
            ForceAttachToCarry(anchor, localPos, localRot);
            return;
        }

        view.Animation.MoveToCarrySlot(anchor, localPos, localRot);
    }

    public void SetSecondaryMotion(Vector3 posOffset, Vector3 eulerOffset, float smoothTime)
    {
        view.Animation.SetSecondaryMotion(posOffset, eulerOffset, smoothTime);
    }

    public void DropToWorld(Vector3 scatterDirection)
    {
        state     = PickupState.World;
        worldAge  = 0f;
        isSettled = false;

        view.SetActivePose(null);
        view.Transform.SetParent(null, true);
        view.Physics.EnableWorldPhysics(UseGravity);
        view.Physics.ApplyScatterVelocity(scatterDirection, MinHorizSpeed, MaxHorizSpeed, MinVertSpeed, MaxVertSpeed, MaxAngularSpeed);
    }

    public void InitializeAsSpendProjectile(string pickupId, Vector3 origin)
    {
        PickupId = pickupId;
        Amount   = 1;
        state    = PickupState.None;

        view.SetActivePose(null);
        view.Transform.SetParent(null, true);
        view.Physics.PlaceAt(origin, Quaternion.identity);
    }

    public void PlaySpendAnimation(Transform target, Action onCompleted)
    {
        state = PickupState.Spending;

        view.Animation.BeginSpend(target, onCompleted);
        view.Transform.SetParent(null, true);
        view.Physics.EnableCarryPhysics();
        view.SetActivePose(() => view.Animation.ApplySpendPose(SpendDuration, JumpPower, SpendSpinSpeed, SpendCurve));
    }

    public void TickWorld(float deltaTime)
    {
        // if (state != PickupState.World || isSettled)
        //     return;
        //
        // worldAge += deltaTime;
        //
        // if (worldAge >= SettleDelay)
        //     SettleInWorld();
    }

    public void Tick(float deltaTime)
    {
        switch (state)
        {
            case PickupState.Collecting:
                if (view.Animation.TickCollect(deltaTime, CollectDuration))
                {
                    var cb     = view.Animation.CollectCompleted;
                    var anchor = view.Animation.CollectAnchor;
                    var lp     = view.Animation.CollectTargetLocalPos;
                    var lr     = view.Animation.CollectTargetLocalRot;

                    ForceAttachToCarry(anchor, lp, lr);
                    cb?.Invoke();
                }
                break;

            case PickupState.Carried:
                view.Animation.TickCarry(deltaTime, MoveToSlotDuration);
                break;

            case PickupState.Spending:
                if (view.Animation.TickSpend(deltaTime, SpendDuration))
                {
                    var cb = view.Animation.SpendCompleted;

                    state = PickupState.None;
                    view.SetActivePose(null);
                    cb?.Invoke();
                }
                break;
        }
    }

    private void SettleInWorld()
    {
        isSettled = true;
        view.Physics.RestOnGround();
    }
}
