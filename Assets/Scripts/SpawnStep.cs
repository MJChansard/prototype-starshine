using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnStep", menuName = "Scriptable Objects/Spawn Step", order = 1)]
public class SpawnStep : ScriptableObject
{
    public GridObject gridObject;   // Reference to the GridObject Prefab
    public Vector2Int SpawnLocation;

    // CONSTRUCTORS
    public SpawnStep (GridObject newGridObject, Vector2Int spawnLocation)
    {
        this.gridObject = newGridObject;
        this.SpawnLocation = spawnLocation;
    }

    // Add a constructor here that accepts required parameters to create a SpawnStep
    // Override the constructor with a SpawnStep parameter
    // Calls original constructor


    // METHODS
    public void Init(GridObject type, Vector2Int location)
    {
        this.gridObject = type;
        this.SpawnLocation = location;
    }
}