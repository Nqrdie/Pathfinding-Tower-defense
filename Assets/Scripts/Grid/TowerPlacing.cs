using System;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacing : MonoBehaviour
{
    private GridManager gridManager;
    private GameObject[] towerPrefab;
    private bool isHoldingTower;
    private bool isHoldingPath;
    private bool isDeleting;
    private GameObject selected;
    private GameObject towerHover;
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D deleteCursor;
    [SerializeField] private Mesh treeMesh;
    [SerializeField] private GameObject selectedPrefab;
    [SerializeField] private Mesh tileMesh;
    [SerializeField] private Mesh pathMesh;
    [SerializeField] private GameObject[] coreTower;
    [SerializeField] private GameObject[] ballistaTower;
    [SerializeField] private GameObject[] cannonTower;
    [SerializeField] private GameObject[] mageTower;

    private void Start()
    {
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        towerPrefab = new GameObject[2];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            isDeleting = !isDeleting;
            isHoldingTower = false;
            isHoldingPath = false;
        }

        if (isDeleting)
        {
            Cursor.SetCursor(deleteCursor, new Vector2(15, 15), CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    public void SelectTower(string tower)
    {
        isHoldingTower = true;

        if (!isDeleting)
        {
            switch (tower)
            {
                case "Core":
                    towerPrefab[0] = coreTower[0];
                    towerPrefab[1] = coreTower[1];
                    break;
                case "Ballista":
                    towerPrefab[0] = ballistaTower[0];
                    towerPrefab[1] = ballistaTower[1];
                    break;
                case "Cannon":
                    towerPrefab[0] = cannonTower[0];
                    towerPrefab[1] = cannonTower[1];
                    break;
                case "Mage":
                    towerPrefab[0] = mageTower[0];
                    towerPrefab[1] = mageTower[1];
                    break;
            }
        }
    }

    public void SelectPath()
    {
        isHoldingPath = true;
        isHoldingTower = false;
    }


   public void TileHovered(GameObject tile)
   {
        if (isHoldingTower)
        {
            towerHover = Instantiate(towerPrefab[0], tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
            selected = Instantiate(towerPrefab[0], tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
        }
        else
        {
            selected = Instantiate(selectedPrefab, tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
        }
    }

    public void TileClicked(GameObject tile)
    {
        Tile tileScript = tile.GetComponent<Tile>();
        //
        // tileScript.isWalkable = !tileScript.isWalkable;
        //
        // tile.GetComponent<MeshFilter>().mesh = treeMesh;


        if (isHoldingTower)
        {
            tileScript.isWalkable = !tileScript.isWalkable;
            Instantiate(towerPrefab[1], tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
            isHoldingTower = false;
            Destroy(towerHover);
        }
        
        if (isHoldingPath)
        {
            gridManager.PlacePath(tileScript.cords);
            tileScript.isWalkable = true;
            tile.GetComponent<Renderer>().material.color = Color.white;
            tile.GetComponent<MeshFilter>().mesh = pathMesh;
        }

        if (isDeleting)
        {
            Cursor.SetCursor(deleteCursor, new Vector2(15, 15), CursorMode.Auto);
            tileScript.isWalkable = false;
            tile.GetComponent<MeshFilter>().mesh = tileMesh;
        }
    }

    public void TileUnHovered()
    {
        Destroy(selected);
        Destroy(towerHover);
    }
}
