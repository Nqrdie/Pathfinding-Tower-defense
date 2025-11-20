using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacing : MonoBehaviour
{
    private GameObject[] towerPrefab;
    private bool isHoldingTower;
    private GameObject selected;
    private GameObject towerHover;
    [SerializeField] private GameObject selectedPrefab;
    [SerializeField] private GameObject[] coreTower;
    [SerializeField] private GameObject[] ballistaTower;
    [SerializeField] private GameObject[] cannonTower;
    [SerializeField] private GameObject[] mageTower;

    private void Start()
    {
        towerPrefab = new GameObject[2];
    }

    public void SelectTower(string tower)
    {
        isHoldingTower = true;

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


   public void TileHovered(GameObject tile)
   {
        if (isHoldingTower)
        {
            towerHover = Instantiate(towerPrefab[0], tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
            selected = Instantiate(selectedPrefab, tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
        }
        else
        {
            selected = Instantiate(selectedPrefab, tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
        }
    }

    public void TileClicked(GameObject tile)
    {
        Tile tileScript = tile.GetComponent<Tile>();
        if (isHoldingTower)
        {
            tileScript.isWalkable = !tileScript.isWalkable;
            Instantiate(towerPrefab[1], tile.transform.position + Vector3.up * 0.2f, Quaternion.identity);
            isHoldingTower = false;
            Destroy(towerHover);
        }
    }

    public void TileUnHovered()
    {
        Destroy(selected);
        Destroy(towerHover);
    }
}
