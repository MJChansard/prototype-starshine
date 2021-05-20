using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/Level Data", order = 2)]
[TypeInfoBox("Level Width and Level Height must take the spawn ring into account.")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class LevelDataRow
    {
        public int levelWidth;
        public int levelHeight;
        public int jumpFuelAmount;
        public int numberOfPhenomenaToSpawn;
        public Vector2Int playerSpawnLocation;
    }

    public LevelDataRow[] LevelTable;

    // CONSTRUCTORS
    public LevelData(int w, int h, int f, int p)
    {
        LevelDataRow ldr = new LevelDataRow();
        ldr.levelWidth = w;
        ldr.levelHeight = h;
        ldr.jumpFuelAmount = f;
        ldr.numberOfPhenomenaToSpawn = p;

        this.LevelTable = new LevelDataRow[1];
        LevelTable[0] = ldr;
    }
}
