using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TowerBase : MonoBehaviour
{
    [Header("Stats")]
    public float range = 5f;
    public float fireRate = 1f; // shots per second
    public float damage = 10f;
    public int cost = 50;
    public int level = 1;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Targeting")]
    public string enemyTag = "Enemy";
    public float targetRefreshRate = 0.25f;

    private float fireCooldown = 0f;
    private Transform currentTarget;
    private Tile placedOnTile;

    private void Start()
    {
        FindPlacedTile();
        StartCoroutine(TargetLoop());
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;
        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            if (dist > range)
            {
                currentTarget = null;
                return;
            }

            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0f;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = 1f / Mathf.Max(0.0001f, fireRate);
            }
        }
    }

    private IEnumerator TargetLoop()
    {
        while (true)
        {
            GetTarget();
            //yield return new WaitForSeconds(targetRefreshRate);
            yield return null;
        }
    }

    private void GetTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform target = null;
        float bestDist = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            float enemyHealth = enemy.GetComponent<Health>().currentHealth;
            if (enemy == null) return;
            float d = Vector3.Distance(pos, enemy.transform.position);
            if (d <= range && d < bestDist && enemyHealth > 0)
            {
                bestDist = d;
                target = enemy.transform;
            }
        }

        currentTarget = target;
    }

    private void Fire()
    {
        if (currentTarget == null) return;

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetTarget(currentTarget, damage);
            }
            else
            {
                Rigidbody rb = projectileGO.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (currentTarget.position - firePoint.position).normalized;
                    rb.linearVelocity = dir * 10f;
                }
            }
        }
        else
        {
            Health healthComp = currentTarget.GetComponent<Health>();
            if (healthComp != null)
                healthComp.TakeDamage(damage);
        }
    }

    private void FindPlacedTile()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.6f);
        for (int i = 0; i < colliders.Length; i++)
        {
            Tile tile = colliders[i].GetComponent<Tile>();
            if (tile != null)
            {
                placedOnTile = tile;
                placedOnTile.isWalkable = false;
                return;
            }
        }
    }

    private void OnDestroy()
    {
        if (placedOnTile != null)
            placedOnTile.isWalkable = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, range);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    public void Upgrade(float damageMultiplier, float rangeBonus, float fireRateBonus)
    {
        level++;
        damage *= damageMultiplier;
        range += rangeBonus;
        fireRate += fireRateBonus;
    }

    public int GetSellValue()
    {
        return Mathf.RoundToInt(cost * 0.5f * level);
    }
}