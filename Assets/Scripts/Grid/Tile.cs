using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        Empty,
        Path,
    }
    
    public TileType tileType = TileType.Empty;
    public Vector2Int cords;
    public Vector2Int connection;
    public bool isWalkable;
    private TowerPlacing towerScript;

    private void Awake()
    {
        towerScript = FindFirstObjectByType<TowerPlacing>();
    }

    public void UpdateCords(Vector2Int cordsInput)
    {
        cords = cordsInput;
        gameObject.name = "Tile " + cordsInput.x + "," + cordsInput.y;
    }

    private void OnMouseDown()
    {
        towerScript.TileClicked(gameObject);
    }

    private void OnMouseEnter()
    {
        if (Input.GetMouseButton(0))
        {
            if (!isWalkable && tileType.Equals(TileType.Empty))
            {
                towerScript.TileClicked(gameObject);
            }
        }
        towerScript.TileHovered(gameObject);
    }

    private void OnMouseExit()
    {
        towerScript.TileUnHovered();
    }
    
}
