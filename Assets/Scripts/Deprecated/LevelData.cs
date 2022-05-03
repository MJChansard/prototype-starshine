using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

//[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/Level Data", order = 2)]
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
        public int numberOfStationsToSpawn;
        //public Vector2Int playerSpawnLocation;

        // CONSTRUCTOR
        public LevelDataRow(int w, int h, int f, int p, int s)
        {
            this.levelWidth = w;
            this.levelHeight = h;
            this.jumpFuelAmount = f;
            this.numberOfPhenomenaToSpawn = p;
            this.numberOfStationsToSpawn = s;
        }
    }

    public LevelDataRow[] LevelTable;

    // CONSTRUCTORS
    public LevelData(int w, int h, int f, int p, int s)
    {
        LevelDataRow ldr = new LevelDataRow(w, h, f, p, s);
        //LevelDataRow ldr = CreateInstance<LevelDataRow>();
        //ldr.levelWidth = w;
        //ldr.levelHeight = h;
        //ldr.jumpFuelAmount = f;
        //ldr.numberOfPhenomenaToSpawn = p;
        //ldr.numberOfStationsToSpawn = s;

        this.LevelTable = new LevelDataRow[1];
        LevelTable[0] = ldr;
    }

    // METHODS
    public void AddRowData (int w, int h, int f, int p, int s)
    {
        if (LevelTable == null)
            LevelTable = new LevelDataRow[1];

        this.LevelTable[0] = new LevelDataRow(w, h, f, p, s);

    }
}
