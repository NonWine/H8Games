using UnityEngine;

public class SimpleProjectileView : MonoBehaviour
{
    private Transform target;
    private float speed;

    public void Launch(Transform target, float speed)
    {
        this.target = target;
        this.speed = Mathf.Max(0.1f, speed);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
            transform.forward = direction.normalized;

        if ((transform.position - target.position).sqrMagnitude <= 0.01f)
            Destroy(gameObject);
    }
}
