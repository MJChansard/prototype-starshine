using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock 
{
    //public Vector3 location;
    public Vector2Int location;
    public bool isOccupied;
    public bool canSpawn;
    public GameObject objectOnBlock;

    // Constructor
    public GridBlock(int x, int y)
    {
        this.location = new Vector2Int(x, y);
        this.isOccupied = false;
        this.objectOnBlock = null;
        this.canSpawn = false;
    }

    #region Methods

    #endregion

}
