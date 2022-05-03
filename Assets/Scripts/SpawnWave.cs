using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSequence", menuName = "Scriptable Objects/Spawn Sequence", order = 1)]
public class SpawnWave : ScriptableObject
{
    public Vector2Int playerSpawnLocation;
    //public List<SpawnStep> hazardSpawnSteps;
    public SpawnRecord[] spawnSteps;

    public SpawnWave Clone()
    {
        //SpawnSequence newSpawnSequence = new SpawnSequence();

        SpawnWave newSpawnSequence = CreateInstance<SpawnWave>();

        newSpawnSequence.playerSpawnLocation = this.playerSpawnLocation;
        newSpawnSequence.spawnSteps = new SpawnRecord[this.spawnSteps.Length];

        for (int i = 0; i < this.spawnSteps.Length; i++)
        {
            newSpawnSequence.spawnSteps[i] = this.spawnSteps[i];
        }

        return newSpawnSequence;
    }
}