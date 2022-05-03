using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelManager : MonoBehaviour
{
    //  #INSPECTOR
    [BoxGroup("LEVEL MANAGER", centerLabel: true)]
    [DisplayAsString] int currentLevel;
    [ShowInInspector] public bool overrideLevelData { get; private set; }

    [ShowIf("overrideLevelData")][TitleGroup("LEVELMANAGER/CREATE SINGLE LEVEL")]
    [ShowIf("overrideLevelData")][SerializeField] int inspectorGridWidth;
    [ShowIf("overrideLevelData")][SerializeField] int inspectorGridHeight;
    [ShowIf("overrideLevelData")][SerializeField] int inspectorJumpFuelAmount;
    [ShowIf("overrideLevelData")][SerializeField] int inspectorPhenomenaSpawnCount;
    [ShowIf("overrideLevelData")][SerializeField] int inspectorStationSpawnCount;
    [ShowIf("overrideLevelData")][TitleGroup("LEVELMANAGER/CUSTOM LEVELS")]
    [ShowIf("overrideLevelData")][SerializeField] LevelRecord[] inspectorLevelData;


    //  #PROPERTIES
    public int CurrentLevel { get { return currentLevel; } }
    public LevelRecord CurrentLevelData
    {
        get
        {
            if (overrideLevelData)
            {
                return levelData[levelIndex];
            }
            else
            {
                LevelRecord manualLevel = LevelRecord.CreateLevelRecord(
                    inspectorGridWidth,
                    inspectorGridHeight,
                    inspectorJumpFuelAmount,
                    inspectorPhenomenaSpawnCount,
                    inspectorStationSpawnCount
                );
                return manualLevel;
            }
        }
    }
    int levelIndex
    {
        get { return currentLevel - 1; }
    }


    //  #FIELDS
    LevelRecord[] levelData;
    
    
    void Awake()    //#RESUME
    {
        levelData = new LevelRecord[21];
        if(overrideLevelData)
        {
            LevelRecord singleLevel = LevelRecord.CreateLevelRecord(
                inspectorGridWidth,
                inspectorGridHeight,
                inspectorJumpFuelAmount,
                inspectorPhenomenaSpawnCount,
                inspectorStationSpawnCount
            );
            
            levelData[0] = singleLevel;
        }

        if (inspectorLevelData.Length > 0)
        {
            for (int i = 0; i < inspectorLevelData.Length; i++)
            {
                levelData[i + 1] = inspectorLevelData[i];
            }
        }
        else
        {
            levelData[1] = LevelRecord.CreateLevelRecord(w: 10, h: 8, f: 1, p: 0, s: 0);
            levelData[1].InitSpawn(1, 2, 0, 5, 1, 1, 0);
            levelData[2] = LevelRecord.CreateLevelRecord(w: 10, h: 8, f: 2, p: 1, s: 1);
        }

    }

    // METHODS
    public void Init()
    {
        currentLevel = 1;
        // Subscribe to end of turn event
    }

    private void EndTurn()
    {
        currentLevel++;
    }
    
    public void AddLevel()
    {

    }

}
