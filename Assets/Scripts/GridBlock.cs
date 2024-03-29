﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GridBlock
{
    public Vector2Int location;
    public List<GameObject> objectsOnBlock;

    
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
    

    // CONSTRUCTOR
    public GridBlock(int x, int y)
    {
        this.location = new Vector2Int(x, y);
        this.objectsOnBlock = new List<GameObject>();
    }
}