using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 12f;
    public float lifeTime = 5f;
    public float retargetRadius = 5f;
    public string enemyTag = "Enemy";

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
        // Target lost or dead → try to retarget
        if (target == null || IsTargetDead(target))
        {
            GetNewTarget();

            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
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
            Health health = target.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private bool IsTargetDead(Transform t)
    {
        Health health = t.GetComponent<Health>();
        return health == null || health.currentHealth <= 0;
    }

    private void GetNewTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, retargetRadius);
        float bestDist = float.MaxValue;
        Transform bestTarget = null;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].CompareTag(enemyTag))
                continue;

            Health h = hits[i].GetComponent<Health>();
            if (h == null || h.currentHealth <= 0)
                continue;

            float d = Vector3.Distance(transform.position, hits[i].transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                bestTarget = hits[i].transform;
            }
        }

        target = bestTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null && other.transform == target)
        {
            HitTarget();
        }
    }
}