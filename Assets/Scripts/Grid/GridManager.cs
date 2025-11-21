using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject tileHolder;
    public Vector2Int gridSize;
    public int unityGridSize;
    [SerializeField] private Mesh straightMesh;
    [SerializeField] private Mesh cornerMesh;


    // TODO: Create a grid around already placed tiles and adding the placed tiles to the grid on the correct location
    // already placed tiles include: Path, Towers, Enemy spawn, Player core
    
    public Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();
    
    private void Awake()
    {
        CreateGrid();
    }
    
    public void CreateGrid()
    {
        DestroyTiles();
        
        if(tileHolder == null)
            tileHolder = new GameObject("TileHolder");
        tileHolder.tag = "Tile";
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int cords = new Vector2Int(x, y);
                GameObject GO = Instantiate(tilePrefab, new Vector3(x * unityGridSize, 0, y * unityGridSize), Quaternion.identity, tileHolder.transform);
                GO.GetComponent<Tile>().UpdateCords(cords);
                grid.Add(cords, GO); 
            }
        }
    }

    private void DestroyTiles()
    {
        if (tileHolder != null)
        { 
            DestroyImmediate(tileHolder);
        }
        grid.Clear();
    }
    
    public void UpdateTileVisual(Vector2Int cords)
    {
        if (!grid.ContainsKey(cords)) return;

        GameObject tileGO = grid[cords];
        Tile tile = tileGO.GetComponent<Tile>();

        if (tile.tileType != Tile.TileType.Path) return;
        
        bool up =    IsPath(cords + Vector2Int.up);
        bool down =  IsPath(cords + Vector2Int.down);
        bool left =  IsPath(cords + Vector2Int.left);
        bool right = IsPath(cords + Vector2Int.right);

        int count = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        Transform t = tileGO.transform;
        
        // Corners
        if (right && down)
        {
            SetCorner(t, 180f);
        }
        else if (down && left)
        {
            SetCorner(t, 270f);
        }
        else if (left && up)
        {
            SetCorner(t, 0f);
        }
        else if (up && right)
        {
            SetCorner(t, 90f);
        }
        
        // Straight tiles
        else if (up && down || up || down)
        {
            SetStraight(t, 0f);
        }
        else if (left && right || right || left)
        {
            SetStraight(t, 90f);
        }
    }
    
    private bool IsPath(Vector2Int pos)
    {
        return grid.ContainsKey(pos) && 
               grid[pos].GetComponent<Tile>().tileType == Tile.TileType.Path;
    }

    private void SetStraight(Transform tile, float rotation)
    {
        tile.GetComponent<MeshFilter>().mesh = straightMesh;
        tile.rotation = Quaternion.Euler(0, rotation, 0);
    }

    private void SetCorner(Transform tile, float rotation)
    {
        tile.GetComponent<MeshFilter>().mesh = cornerMesh;
        tile.rotation = Quaternion.Euler(0, rotation, 0);
    }
    
    public void PlacePath(Vector2Int coord)
    {
        Tile tile = grid[coord].GetComponent<Tile>();
        tile.tileType = Tile.TileType.Path;
        
        UpdateTileVisual(coord);
        UpdateTileVisual(coord + Vector2Int.up);
        UpdateTileVisual(coord + Vector2Int.down);
        UpdateTileVisual(coord + Vector2Int.left);
        UpdateTileVisual(coord + Vector2Int.right);
    }
}


