using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "LevelRecord", menuName = "Scriptable Objects/Level Record", order = 2)]
[TypeInfoBox("Level Width and Level Height must take the spawn ring into account.")]
public class LevelRecord : ScriptableObject
{
    //  #FIELDS
    public int width;
    public int height;
    public int jumpFuelAmount;
    public int numberOfPhenomenaToSpawn;
    public int numberOfStationsToSpawn;
    //public Vector2Int playerSpawnLocation;

    public int minObjectsPerWave;           // Value to set lowest number of object to spawn in a wave
    public int maxObjectsPerWaveWave;
    public int minGridObjectsOnGrid;
    public int maxGridObjectsOnGrid;        // Value to cap number of objects on Grid
    public int maxSpawnPerSide;
    public int maxSidesEligibleForSpawn;
    public int maxInteriorSpawn;

    //  #PROPERTIES
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


    // CONSTRUCTOR
    public LevelRecord(int w, int h, int f, int p, int s)
    {
        width = w;
        height = h;
        jumpFuelAmount = f;
        numberOfPhenomenaToSpawn = p;
        numberOfStationsToSpawn = s;
    }

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

    public void InitSpawn(int _minObjectsPerWave, int _maxObjectsPerWaveWave, int _minGridObjectsOnGrid, int _maxGridObjectsOnGrid, int _maxSpawnPerSide, int _maxSidesEligibleForSpawn, int _maxInteriorSpawn)
    {
        minObjectsPerWave           = _minObjectsPerWave;
        maxObjectsPerWaveWave       = _maxObjectsPerWaveWave;
        minGridObjectsOnGrid        = _minGridObjectsOnGrid;
        maxGridObjectsOnGrid        = _maxGridObjectsOnGrid;
        maxSpawnPerSide             = _maxSpawnPerSide;
        maxSidesEligibleForSpawn    = _maxSidesEligibleForSpawn;
        maxInteriorSpawn            = _maxInteriorSpawn;
    }
}
