using System;
using UnityEngine;
using Zenject;

public class PickupItemController
{
    private readonly PickupItemView view;
    private readonly PickupVisualConfig visualConfig;
    private PickupState state;

    public string PickupId { get; private set; }
    public int Amount { get; private set; }
    public bool IsRented => view.IsRented;
    public bool IsWorldPickup => state == PickupState.World;

    public PickupItemView View => view;

    public PickupItemController(PickupItemView view, PickupVisualConfig visualConfig)
    {
        this.view = view;
        this.visualConfig = visualConfig;
    }

    public class Factory : PlaceholderFactory<PickupItemView, PickupVisualConfig, PickupItemController>
    {
    }

    public void InitializeAsWorldItem(string pickupId, int amount, Vector3 position, Vector3 scatterDirection)
    {
        PickupId = pickupId;
        Amount = amount;
        state = PickupState.World;
        view.SetActivePose(null);
        view.Transform.SetParent(null, true);
        view.Physics.PlaceAt(position, Quaternion.identity);
        view.Physics.EnableWorldPhysics(visualConfig.UseGravity);
        view.Physics.ApplyScatterVelocity(scatterDirection, visualConfig.MinHorizSpeed, visualConfig.MaxHorizSpeed, visualConfig.MinVertSpeed, visualConfig.MaxVertSpeed, visualConfig.MaxAngularSpeed);
    }

    public void PlayCollectAnimation(Transform anchor, Vector3 localTargetPos, Quaternion localTargetRot, Action onCompleted)
    {
        state = PickupState.Collecting;
        view.Animation.BeginCollect(anchor, localTargetPos, localTargetRot, onCompleted);
        view.Transform.SetParent(null, true);
        view.Physics.EnableCarryPhysics();
        view.SetActivePose(() => view.Animation.ApplyCollectPose(visualConfig.CollectDuration, visualConfig.ArcHeight, visualConfig.CollectCurve));
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

    public void InitializeAsSpendProjectile(string pickupId, Vector3 origin)
    {
        PickupId = pickupId;
        Amount = 1;
        state = PickupState.None;
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
        view.SetActivePose(() => view.Animation.ApplySpendPose(visualConfig.SpendDuration, visualConfig.JumpPower, visualConfig.SpendSpinSpeed, visualConfig.SpendCurve));
    }

    public void Tick(float deltaTime)
    {
        switch (state)
        {
            case PickupState.Collecting:
                if (view.Animation.TickCollect(deltaTime, visualConfig.CollectDuration))
                {
                    var cb = view.Animation.CollectCompleted;
                    var anchor = view.Animation.CollectAnchor;
                    var lp = view.Animation.CollectTargetLocalPos;
                    var lr = view.Animation.CollectTargetLocalRot;

                    ForceAttachToCarry(anchor, lp, lr);
                    cb?.Invoke();
                }
                break;

            case PickupState.Carried:
                view.Animation.TickCarry(deltaTime, visualConfig.MoveToSlotDuration);
                break;

            case PickupState.Spending:
                if (view.Animation.TickSpend(deltaTime, visualConfig.SpendDuration))
                {
                    var cb = view.Animation.SpendCompleted;

                    state = PickupState.None;
                    view.SetActivePose(null);
                    cb?.Invoke();
                }
                break;
        }
    }
}
