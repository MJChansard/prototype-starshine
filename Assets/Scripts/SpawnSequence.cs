using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSequence", menuName = "Scriptable Objects/Spawn Sequence", order = 1)]
public class SpawnSequence : ScriptableObject
{
    public Vector2Int playerSpawnLocation;
    //public List<SpawnStep> hazardSpawnSteps;
    public SpawnStep[] hazardSpawnSteps;
}

[CreateAssetMenu(fileName = "SpawnStep", menuName = "Scriptable Objects/Spawn Step", order = 1)]
public class SpawnStep : ScriptableObject
{
    public Hazard.HazardType HazardType;
    public Vector2Int SpawnLocation;
}
