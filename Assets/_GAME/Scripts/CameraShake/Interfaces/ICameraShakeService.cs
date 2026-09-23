using UnityEngine;

// Narrow contract between "something happened in gameplay" and "the camera
// shakes". Keeps callers free of Cinemachine and lets the shake be swapped for
// a null object when no shaker is present in the scene.
public interface ICameraShakeService
{
    // worldDirection points from the damage source towards the camera target;
    // pass Vector3.zero for a shake with no directional bias.
    void Shake(CameraShakeSettings settings, float scale, Vector3 worldDirection);

    void StopAll();
}
