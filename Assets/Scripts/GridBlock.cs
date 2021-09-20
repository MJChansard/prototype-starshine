using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GridBlock : MonoBehaviour
{
    [ReadOnly] public Vector2Int location;
    [ReadOnly] public List<GameObject> objectsOnBlock;
    [ReadOnly] public bool canSpawn;

    
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
    private GameObject debugRenderPoint;

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
            Loot loot = objectsOnBlock[i].GetComponent<Loot>();
            if (loot != null) countOfLootItems += 1;
        }
        return countOfLootItems > 0;
    }

    private bool DoesContainObjectPreventingPlayerMovement()
    {
        int countOfObjects = 0;
        for (int i = 0; i < objectsOnBlock.Count; i++)
        {
            GridObject gridObject = objectsOnBlock[i].GetComponent<GridObject>();
            // if(gridObject != null && hazard.hazardType != Hazard.Type.AmmoCrate) countOfObjects += 1;
            if (gridObject != null)
            {
                if (gridObject as SupplyCrate == null) countOfObjects += 1;
            }
        }
        return countOfObjects > 0;
    }

    // CONSTRUCTOR
    public GridBlock(int x, int y)
    {
        this.location = new Vector2Int(x, y);
        this.objectsOnBlock = new List<GameObject>();
        this.canSpawn = false;
    }
}
