using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock 
{
    public Vector2Int location;
    public GameObject objectOnBlock;
    public bool isOccupied;
    
    public bool canSpawn;
    public bool validMoveUp;
    public bool validMoveDown;
    public bool validMoveLeft;
    public bool validMoveRight;
    

    // Constructor
    public GridBlock(int x, int y)
    {
        this.location = new Vector2Int(x, y);
        this.objectOnBlock = null;
        this.isOccupied = false;
        this.canSpawn = false;
    }

    #region Methods

    #endregion

}
