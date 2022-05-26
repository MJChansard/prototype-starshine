using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "LevelRecord", menuName = "Scriptable Objects/Level Record", order = 2)]
[TypeInfoBox("Level Width and Level Height must take the spawn ring into account.")]
public class LevelRecord : ScriptableObject
{
    //  #LEVEL TRAITS
    public int width;
    public int height;
    public int jumpFuelAmount;
    public int numberOfPhenomenaToSpawn;
    public int numberOfStationsToSpawn;
    //public Vector2Int playerSpawnLocation;


    public int BoundaryLeftActual
    {
        get { return -(width / 2); }
    }
    public int BoundaryRightActual
    {
        get
        {
            if (width % 2 == 0)
                return (width / 2) - 1;
            else
                return (width / 2);
        }
    }
    public int BoundaryTopActual
    {
        get
        {
            if (height % 2 == 0)
                return (height / 2) - 1;
            else
                return height / 2;
        }
    }
    public int BoundaryBottomActual
    {
        get { return -(height / 2); }
    }

    public int BoundaryLeftPlay
    {
        get { return BoundaryLeftActual + 1; }
    }
    public int BoundaryRightPlay
    {
        get { return BoundaryRightActual - 1; }
    }
    public int BoundaryTopPlay
    {
        get { return BoundaryTopActual - 1; }
    }
    public int BoundaryBottomPlay
    {
        get { return BoundaryBottomActual + 1; }
    }

    public int LevelAreaPlay { get { return (width - 2) * (height - 2); } }


    //  #SPAWN TRAITS
    public int minObjectsPerWave;
    public int maxObjectsPerWave;
    public int minGridObjectsOnGrid;
    public bool scarcity;
    public bool saturation;

    public GridBorder[] bordersEligibleForSpawn;
    public GridObject[] eligibleForSpawn;


    public int MaxObjectsOnGrid { get { return LevelAreaPlay / 2; } }
    public int MaxInteriorSpawns { get { return LevelAreaPlay / 4; } }
    public int MaxSpawnOnBorder(GridBorder border)
    {
        int result;
        if (border == GridBorder.Top || border == GridBorder.Bottom)
            result = width - 2;
        else
            result = height - 2;
        
        return result;
    }


    // CONSTRUCTOR
    public static LevelRecord CreateLevelRecord(int w, int h, int f, int p, int s)
    {
        LevelRecord lr = LevelRecord.CreateInstance<LevelRecord>();
        lr.width = w;
        lr.height = h;
        lr.jumpFuelAmount = f;
        lr.numberOfPhenomenaToSpawn = p;
        lr.numberOfStationsToSpawn = s;

        return lr;
    }

    public void InitSpawn(int _minObjectsPerWave, int _maxObjectsPerWave, int _minGridObjectsOnGrid, GridBorder[] _bordersEligibleForSpawn)
    {
        minObjectsPerWave           = _minObjectsPerWave;
        maxObjectsPerWave           = _maxObjectsPerWave;
        minGridObjectsOnGrid        = _minGridObjectsOnGrid;
        bordersEligibleForSpawn     = _bordersEligibleForSpawn;
    }
}
