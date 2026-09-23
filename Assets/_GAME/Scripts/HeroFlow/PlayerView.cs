using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerView : MonoBehaviour, IPickupCarryAnchorProvider
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private Transform cameraLookAtAnchor;
    [SerializeField] private Transform combatCameraAnchor;
    [SerializeField] private Transform carryAnchor;
    [SerializeField] private HealthView healthBarView;
    [SerializeField] private SimpleProjectileView projectilePrefab;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;

    [Inject] private Joystick movementJoystick;

    public event Action Died;
    public event Action<float, Vector3> DamageReceived;

    public Transform AttackOrigin => attackPoint;
    public Transform CameraAnchor => cameraAnchor;
    public Transform CameraLookAtAnchor => cameraLookAtAnchor != null ? cameraLookAtAnchor : cameraAnchor;
    public Transform CombatCameraAnchor => combatCameraAnchor != null ? combatCameraAnchor : cameraAnchor;
    public Transform CarryAnchor => carryAnchor;
    public Joystick MovementJoystick => movementJoystick;
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;
    public IHealthView HealthBar => healthBarView;

    public bool TryGetAnchor(out Transform anchor)
    {
        anchor = carryAnchor;
        return anchor != null;
    }
}
