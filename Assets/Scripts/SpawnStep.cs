using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnStep", menuName = "Scriptable Objects/Spawn Step", order = 1)]
public class SpawnStep : ScriptableObject
{
    //public Hazard.Type HazardType;
    public GridObject gridObject;   // Reference to the GridObject Prefab
    public Vector2Int SpawnLocation;

    // CONSTRUCTORS V1
/*
    public SpawnStep (Hazard.Type hazardType, Vector2Int spawnLocation)
    {
        this.HazardType = hazardType;
        this.SpawnLocation = spawnLocation;
    }

    public SpawnStep(SpawnStep spawnData)
    {
        this.HazardType = spawnData.HazardType;
        this.SpawnLocation = spawnData.SpawnLocation;
    }
*/
    // CONSTRUCTORS V2
    public SpawnStep (GridObject newGridObject, Vector2Int spawnLocation)
    {
        this.gridObject = newGridObject;
        this.SpawnLocation = spawnLocation;

        /*
        if (gridObject.GetType() == typeof(Hazard))
        {
            Hazard hazard = gridObject as Hazard;
            this.ObjectType = Hazard.Type;
        }
        */
    }

    // Add a constructor here that accepts required parameters to create a SpawnStep
    // Override the constructor with a SpawnStep parameter
    // Calls original constructor


    // METHODS
    public void Init(GridObject type, Vector2Int location)
    {
        /*
        if (type.GetType() == typeof(Hazard))
        {
            this.hazardType = type;
            this.SpawnLocation = location;
        }
        */
        this.gridObject = type;
        this.SpawnLocation = location;
    }
    
    /*
    public SpawnStep Clone()
    {
        SpawnStep spawnStepClone = CreateInstance<SpawnStep>();

        spawnStepClone.HazardType = this.HazardType;
        spawnStepClone.SpawnLocation = this.SpawnLocation;

        return spawnStepClone;
    }
    */
}