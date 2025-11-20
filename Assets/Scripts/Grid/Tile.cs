using UnityEngine;

public class Tile : MonoBehaviour
{
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
        towerScript.TileClicked(this.gameObject);
    }

    private void OnMouseEnter()
    {
        towerScript.TileHovered(this.gameObject);
    }

    private void OnMouseExit()
    {
        towerScript.TileUnHovered();
    }
    
}
