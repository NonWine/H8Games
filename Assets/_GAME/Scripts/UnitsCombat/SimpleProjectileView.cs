using System;
using UnityEngine;

public class SimpleProjectileView : MonoBehaviour
{
    private Transform target;
    private float speed;
    private Action onHit;
    private bool hitResolved;

    public void Launch(Transform target, float speed, Action onHit = null)
    {
        this.target = target;
        this.speed = Mathf.Max(0.1f, speed);
        this.onHit = onHit;
        hitResolved = false;
    }

    private void Update()
    {
        if (target == null)
        {
            DestroyProjectile();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
            transform.forward = direction.normalized;

        if ((transform.position - target.position).sqrMagnitude <= 0.01f)
        {
            if (!hitResolved)
            {
                hitResolved = true;
                onHit?.Invoke();
            }

            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && audio.isPlaying)
        {
            audio.transform.SetParent(null);
            Destroy(audio.gameObject, audio.clip != null ? audio.clip.length : 2f);
        }

        Destroy(gameObject);
    }
}
