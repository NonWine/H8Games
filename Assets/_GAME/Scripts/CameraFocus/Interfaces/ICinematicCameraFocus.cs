using DG.Tweening;
using UnityEngine;

public interface ICinematicCameraFocus
{
    bool IsActive { get; }

    Tween Focus(Transform target);

    Tween Release();
}
