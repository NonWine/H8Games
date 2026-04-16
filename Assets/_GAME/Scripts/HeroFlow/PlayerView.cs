using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private WorldHealthBarView healthBarView;
    [SerializeField] private SimpleProjectileView projectilePrefab;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    
    [Inject] private Joystick movementJoystick;

    public event Action Died;
    public event Action<float, Vector3> DamageReceived;

    public Transform AttackOrigin => attackPoint;
    public Transform CameraAnchor => cameraAnchor;
    public Joystick MovementJoystick => movementJoystick;
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;
}
