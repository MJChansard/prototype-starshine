using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSequence", menuName = "Scriptable Objects/Spawn Sequence", order = 1)]
public class SpawnSequence : ScriptableObject
{
    public Vector2Int playerSpawnLocation;
    //public List<SpawnStep> hazardSpawnSteps;
    public SpawnStep[] hazardSpawnSteps;

    public SpawnSequence Clone()
    {
        //SpawnSequence newSpawnSequence = new SpawnSequence();

        SpawnSequence newSpawnSequence = CreateInstance<SpawnSequence>();

        newSpawnSequence.playerSpawnLocation = this.playerSpawnLocation;
        newSpawnSequence.hazardSpawnSteps = new SpawnStep[this.hazardSpawnSteps.Length];

        for (int i = 0; i < this.hazardSpawnSteps.Length; i++)
        {
            newSpawnSequence.hazardSpawnSteps[i] = this.hazardSpawnSteps[i];
        }

        return newSpawnSequence;
    }
}