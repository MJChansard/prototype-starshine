using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock 
{
    public Vector3 location;
    public bool isOccupied;
    public GameObject objectOnBlock;

    // Constructor
    public GridBlock(float x, float y, float z)
    {
        this.location = new Vector3(x, y, z);
        this.isOccupied = false;
        this.objectOnBlock = null;
    }

    #region Methods

    #endregion

}
