using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private enum States {
        Idle,
        Walking,
        Attacking,
        Dead
    }

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
    public float searchDelay;

    private void Awake()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        //StartCoroutine(FindPath());
    }

    void Update()
    {
        switch (currentState)
        {
            case States.Idle:
                animator.SetInteger("State", 0);
                break;
            case States.Walking:
                animator.SetInteger("State", 1);
                break;
            case States.Attacking:
                animator.SetInteger("State", 2);
                break;
            case States.Dead:
                animator.SetInteger("State", 3);
                break;
            default:
                currentState = States.Idle;
                break;
        }
    }

    #region Pathfinding
    public void StartPathfinding()
    {
        StartCoroutine(FindPath());
    }

    public IEnumerator FindPath()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        foreach (GameObject tile in gridManager.grid.Values)
        {
            if (tile.GetComponent<Tile>().isWalkable)
                tile.GetComponentInChildren<Renderer>().material.color = Color.grey;
            else
                tile.GetComponentInChildren<Renderer>().material.color = Color.black;
        }

        startCords = startCordsInput;
        targetCords = targetCordsInput;
        pathSuccess = false;

        tileToSearch.Clear();
        searchedTiles.Clear();
        path.Clear();

        tileToSearch.Add(startCords);

        while (tileToSearch.Count > 0 && !pathSuccess)
        {
            Vector2Int currentTile = tileToSearch[0];
            tileToSearch.RemoveAt(0);
            searchedTiles.Add(currentTile);

            if (currentTile == targetCords)
            {
                pathSuccess = true;
                RetracePath(startCords, targetCords);
                break;
            }


            foreach (Vector2Int neighbour in GetNeighbours(currentTile))
            {
                if (searchedTiles.Contains(neighbour) || tileToSearch.Contains(neighbour))
                    continue;

                Tile neighbourTile = gridManager.grid[neighbour].GetComponent<Tile>();

                if (!neighbourTile.isWalkable)
                {
                    neighbourTile.GetComponentInChildren<Renderer>().material.color = Color.red;
                    searchedTiles.Add(neighbour);
                    continue;
                }

                neighbourTile.connection = currentTile;
                tileToSearch.Add(neighbour);

                gridManager.grid[neighbour].GetComponentInChildren<Renderer>().material.color = Color.yellow;
                yield return new WaitForSeconds(searchDelay);
            }
        }

        if (!pathSuccess)
        {
            Debug.LogWarning("No path found!");
        }

        stopwatch.Stop();
        Debug.Log($"Pathfinding completed in {stopwatch.ElapsedMilliseconds} ms");
    }

    private List<Vector2Int> GetNeighbours(Vector2Int tileCords)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        Vector2Int[] directions = {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(1, 0),   // Right
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 1),   // Up-Right
            new Vector2Int(-1, 1),  // Up-Left
            new Vector2Int(1, -1),  // Down-Right
            new Vector2Int(-1, -1)  // Down-Left
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourCords = tileCords + direction;

            if (!gridManager.grid.ContainsKey(neighbourCords))
                continue;

            if (direction.x != 0 && direction.y != 0)
            {
                Vector2Int side1 = new Vector2Int(tileCords.x + direction.x, tileCords.y);
                Vector2Int side2 = new Vector2Int(tileCords.x, tileCords.y + direction.y);

                bool side1Blocked = !gridManager.grid.ContainsKey(side1) || !gridManager.grid[side1].GetComponent<Tile>().isWalkable;
                bool side2Blocked = !gridManager.grid.ContainsKey(side2) || !gridManager.grid[side2].GetComponent<Tile>().isWalkable;

                if (side1Blocked && side2Blocked)
                    continue;
            }

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
        foreach (var tilePos in path)
        {
            GameObject tileObj = gridManager.grid[tilePos];
            tileObj.GetComponentInChildren<Renderer>().material.color = Color.green;

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
    }
    #endregion
}
