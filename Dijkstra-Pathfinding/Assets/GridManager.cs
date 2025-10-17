using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject tileHolder;
    public Vector2Int gridSize;
    public int unityGridSize;
    
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
}


