using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 12f;
    public float lifeTime = 5f;
    private Transform target;
    private float damage;

    public void SetTarget(Transform t, float dmg)
    {
        target = t;
        damage = dmg;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target == null)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            return;
        }

        Vector3 dir = (target.position - transform.position);
        float distThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distThisFrame, Space.World);
        transform.LookAt(target);
    }

    private void HitTarget()
    {
        if (target != null)
        {
            EnemyBehaviour enemy = target.GetComponent<EnemyBehaviour>();
            Health enemyHealth = target.GetComponent<Health>();
            if (enemy != null)
                enemyHealth.TakeDamage(damage);
            else
            {
                Health health = target.GetComponent<Health>();
                if (health != null) health.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null && other.transform == target)
        {
            HitTarget();
        }
    }
}