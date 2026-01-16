using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class EnemyBehaviour : MonoBehaviour
{
    private enum States {
        Idle,
        Walking,
        Attacking,
        Dead
    }

    [Header("Speed settings")]
    private Animator animator;
    private States currentState;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    private Rigidbody rb;
    
    public List<Vector2Int> tileToSearch = new List<Vector2Int>();
    public List<Vector2Int> searchedTiles = new List<Vector2Int>();
    public List<Vector2Int> path = new List<Vector2Int>();
    public Vector2Int startCords;
    public Vector2Int targetCords;
    public bool pathSuccess;
    private GridManager gridManager;
    public Vector2Int startCordsInput;
    public Vector2Int targetCordsInput;
    private Health healthComp;
    public float searchDelay;
    
    private float baseMoveSpeed;
    private float healthMultiplier = 1f;
    private float speedMultiplier = 1f;

    private Coroutine pathfindingCoroutine;

    private void Awake()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        healthComp = GetComponent<Health>();
        baseMoveSpeed = moveSpeed;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null;
        pathfindingCoroutine = StartCoroutine(FindPath());
    }

    void Update()
    {
        switch (currentState)
        {
            case States.Idle:
                if (animator) animator.SetInteger("State", 0);
                break;
            case States.Walking:
                if (animator) animator.SetInteger("State", 1);
                break;
            case States.Attacking:
                if (animator) animator.SetInteger("State", 2);
                break;
            case States.Dead:
                if (animator) animator.SetInteger("State", 3);
                break;
            default:
                currentState = States.Idle;
                break;
        }
    }
    
    public void ApplyMultipliers(Vector2 multipliers)
    {
        healthMultiplier = multipliers.x;
        speedMultiplier = multipliers.y;

        healthComp.currentHealth = healthComp.maxHealth * healthMultiplier;
        moveSpeed = baseMoveSpeed * speedMultiplier;
    }

    public void StartPathfinding()
    {
        if (pathfindingCoroutine != null)
            StopCoroutine(pathfindingCoroutine);
        pathfindingCoroutine = StartCoroutine(FindPath());
    }

    #region Pathfinding
    public IEnumerator FindPath()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Dictionary<Vector2Int, GameObject> grid = gridManager.grid;
        
        foreach (var kv in grid)
        {
            GameObject go = kv.Value;
            Tile tile = go.GetComponent<Tile>();
            Renderer rend = go.GetComponentInChildren<Renderer>();
            if (tile != null && rend != null)
                rend.material.color = tile.isWalkable ? Color.yellow : Color.red;
        }

        startCords = startCordsInput;
        targetCords = targetCordsInput;
        pathSuccess = false;

        Queue<Vector2Int> nextTile = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        nextTile.Enqueue(startCords);
        visited.Add(startCords);

        while (nextTile.Count > 0 && !pathSuccess)
        {
            Vector2Int current = nextTile.Dequeue();

            if (current == targetCords)
            {
                pathSuccess = true;
                RetracePath(startCords, targetCords);
                break;
            }

            foreach (Vector2Int neighbour in GetNeighbours(current))
            {
                if (visited.Contains(neighbour))
                    continue;

                if (!grid.TryGetValue(neighbour, out GameObject neighbourGO))
                {
                    visited.Add(neighbour);
                    continue;
                }

                Tile neighbourTile = neighbourGO.GetComponent<Tile>();
                Renderer neighbourRend = neighbourGO.GetComponentInChildren<Renderer>();

                if (neighbourTile == null)
                {
                    visited.Add(neighbour);
                    continue;
                }

                if (!neighbourTile.isWalkable)
                {
                    if (neighbourRend != null)
                        neighbourRend.material.color = Color.black;
                    visited.Add(neighbour);
                    continue;
                }

                neighbourTile.connection = current;
                nextTile.Enqueue(neighbour);
                visited.Add(neighbour);

                if (neighbourRend != null)
                    neighbourRend.material.color = Color.yellow;
            }

            if (searchDelay > 0f)
                yield return new WaitForSeconds(searchDelay);
            else
                yield return null;
        }

        if (!pathSuccess)
            Debug.LogWarning("No path found!");

        stopwatch.Stop();
        Debug.Log($"Pathfinding completed in {stopwatch.ElapsedMilliseconds} ms");
    }

    private List<Vector2Int> GetNeighbours(Vector2Int tileCords)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourCords = tileCords + direction;

            if (!gridManager.grid.ContainsKey(neighbourCords))
                continue;

            neighbours.Add(neighbourCords);
        }

        return neighbours;
    }

    private void RetracePath(Vector2Int startCords, Vector2Int targetCords)
    {
        Vector2Int currentTile = targetCords;
        path.Clear();
        path.Add(currentTile);

        while (currentTile != startCords)
        {
            Tile currentTileComponent = gridManager.grid[currentTile].GetComponent<Tile>();
            Vector2Int connection = currentTileComponent.connection;

            currentTile = connection;

            path.Add(currentTile);
        }

        path.Reverse();

        StartCoroutine(MoveAlongPath());
    }

    private IEnumerator MoveAlongPath()
    {
        Vector3 endPos = Vector3.zero;
        foreach (Vector2Int tilePos in path)
        {
            if (currentState == States.Dead) yield break;

            GameObject tileObj = gridManager.grid[tilePos];
            Renderer rend = tileObj.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = Color.green;

            Vector3 target = tileObj.transform.position;
            while (Vector3.Distance(rb.position, target) > 0.5f)
            {
                Vector3 targetPos = Vector3.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime);
                rb.MovePosition(targetPos);
                currentState = States.Walking;

                Vector3 direction = (target - rb.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
                }

                yield return new WaitForFixedUpdate();
            }
            endPos = target;
        }
        rb.MovePosition(endPos);
        
        currentState = States.Attacking;
        Destroy(gameObject);
    }
    #endregion
}