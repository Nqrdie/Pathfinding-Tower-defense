using System;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int cords;
    public Vector2Int connection;
    public bool isWalkable;
    [SerializeField] private GameObject selectedPrefab;
    private GameObject selected;
    [SerializeField] private Mesh wallMesh;
    private Mesh tileMesh;
    
    [SerializeField] private TextMeshPro label;

    private void Awake()
    {
        tileMesh = GetComponent<MeshFilter>().mesh;
    }

    public void UpdateCords(Vector2Int cordsInput)
    {
        cords = cordsInput;
        gameObject.name = "Tile " + cordsInput.x + "," + cordsInput.y;
    }


    private void OnMouseDown()
    {
        isWalkable = !isWalkable;
        gameObject.GetComponent<MeshFilter>().mesh = isWalkable ? tileMesh : wallMesh;
    }

    private void OnMouseEnter()
    {
        selected = Instantiate(selectedPrefab, transform.position + Vector3.up * 0.2f, Quaternion.identity);
    }

    private void OnMouseExit()
    {
        Destroy(selected);
    }
    
}
