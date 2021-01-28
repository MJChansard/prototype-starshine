using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnStep", menuName = "Scriptable Objects/Spawn Step", order = 1)]
public class SpawnStep : ScriptableObject
{   
    public Hazard.HazardType HazardType;
    public Vector2Int SpawnLocation;

    // CONSTRUCTORS
    public SpawnStep (Hazard.HazardType hazardType, Vector2Int spawnLocation)
    {
        this.HazardType = hazardType;
        this.SpawnLocation = spawnLocation;
    }

    public SpawnStep(SpawnStep spawnData)
    {
        this.HazardType = spawnData.HazardType;
        this.SpawnLocation = spawnData.SpawnLocation;
    }
    // Add a constructor here that accepts required parameters to create a SpawnStep
    // Override the constructor with a SpawnStep parameter
    // Calls original constructor


    // METHODS
    public void Init(Hazard.HazardType type, Vector2Int location)
    {
        this.HazardType = type;
        this.SpawnLocation = location;
    }
    
    public SpawnStep Clone()
    {
        SpawnStep spawnStepClone = CreateInstance<SpawnStep>();

        spawnStepClone.HazardType = this.HazardType;
        spawnStepClone.SpawnLocation = this.SpawnLocation;

        return spawnStepClone;
    }
}