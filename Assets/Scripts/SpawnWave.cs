using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSequence", menuName = "Scriptable Objects/Spawn Sequence", order = 1)]
public class SpawnWave : ScriptableObject
{
    public SpawnRecord[] spawns;

    public static SpawnWave CreateSpawnWave(int spawnCount)
    {
        SpawnWave result = ScriptableObject.CreateInstance<SpawnWave>();
        result.spawns = new SpawnRecord[spawnCount];
        return result;
    }
    
    public SpawnWave Clone()
    {
        SpawnWave newSpawnSequence = CreateInstance<SpawnWave>();
        newSpawnSequence.spawns = new SpawnRecord[this.spawns.Length];

        for (int i = 0; i < this.spawns.Length; i++)
        {
            newSpawnSequence.spawns[i] = this.spawns[i];
        }

        return newSpawnSequence;
    }
}