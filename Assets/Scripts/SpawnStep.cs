using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "SpawnStep", menuName = "Scriptable Objects/Spawn Step", order = 1)]
public class SpawnStep : ScriptableObject
{
    [AssetSelector(Paths = "Assets/Prefabs/GridObjects")]
    public GridObject gridObject;
    public Vector2Int SpawnLocation;


    // METHODS
    public void Init(GridObject type, Vector2Int location)
    {
        this.gridObject = type;
        this.SpawnLocation = location;
    }
}