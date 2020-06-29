using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock 
{
    public Vector2Int location;
    public List<GameObject> objectsOnBlock;
    public bool canSpawn;

    public bool IsOccupied
    {
        get
        {
            if (objectsOnBlock == null) return false;
            else return objectsOnBlock.Count > 0;
        }
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
