using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock 
{
    public Vector2Int location;
    public List<GameObject> objectsOnBlock;
    public bool canSpawn;

    private GameObject debugRenderPoint;
    public GameObject DebugRenderPoint
    {
        get
        {
            return debugRenderPoint;
        }
        set
        {
            debugRenderPoint = value;
        }
    }

    public bool IsAvailableForPlayer
    {
        get
        {
            if (!canSpawn && !DoesContainObjectPreventingPlayerMovement()) return true;
            else return false;
        }
    }

    private bool DoesContainLoot()
    {
        int countOfLootItems = 0;
        for (int i = 0; i < objectsOnBlock.Count; i++)
        {
            LootData loot = objectsOnBlock[i].GetComponent<LootData>();
            if (loot != null) countOfLootItems += 1;
        }
        return countOfLootItems > 0;
    }

    private bool DoesContainObjectPreventingPlayerMovement()
    {
        int countOfObjects = 0;
        for (int i = 0; i < objectsOnBlock.Count; i++)
        {
            Hazard hazard = objectsOnBlock[i].GetComponent<Hazard>();
            if (hazard != null && hazard.hazardType != Hazard.HazardType.AmmoCrate) countOfObjects += 1;
        }
        return countOfObjects > 0;
    }

    // Constructor
    public GridBlock(int x, int y)
    {
        this.location = new Vector2Int(x, y);
        this.objectsOnBlock = new List<GameObject>();
        this.canSpawn = false;
    }

    #region Methods

    #endregion

}
