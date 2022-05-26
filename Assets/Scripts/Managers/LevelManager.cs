using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelManager : MonoBehaviour
{
    //  #INSPECTOR
    [BoxGroup("GENERAL SETTINGS", centerLabel: true)]
    [ShowInInspector] int currentLevel;
    [BoxGroup("GENERAL SETTINGS")][ShowInInspector]public bool overrideLevelData { get; private set; }

    [BoxGroup("LEVEL CONFIGURATION", centerLabel: true)]
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorGridWidth;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorGridHeight;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorJumpFuelAmount;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorPhenomenaSpawnCount;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorStationSpawnCount;
    
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CUSTOM LEVELS")]
    [ShowIf("overrideLevelData")][SerializeField] LevelRecord[] inspectorLevelData;


    //  #PROPERTIES
    public int CurrentLevel { get { return currentLevel; } }
    public LevelRecord CurrentLevelData
    {
        get
        {
            if (levelData[0] != null)
                return levelData[0];
            else
                return levelData[1];
        }
    }
    int levelIndex
    {
        get { return currentLevel - 1; }
    }


    //  #FIELDS
    LevelRecord[] levelData;
    
    
    void Awake()    
    {
        currentLevel = 1;
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
            levelData[1].InitSpawn(
                _minObjectsPerWave:1,
                _maxObjectsPerWave:2,
                _minGridObjectsOnGrid: 0,
                _bordersEligibleForSpawn: new GridBorder[4] { GridBorder.Top, GridBorder.Bottom, GridBorder.Left, GridBorder.Right }
            );
            levelData[2] = LevelRecord.CreateLevelRecord(w: 10, h: 8, f: 2, p: 1, s: 1);
            levelData[1].InitSpawn(
               _minObjectsPerWave: 1,
               _maxObjectsPerWave: 2,
               _minGridObjectsOnGrid: 0,
               _bordersEligibleForSpawn: new GridBorder[4] {GridBorder.Top, GridBorder.Bottom, GridBorder.Left, GridBorder.Right}
           );
        }
    }

    // METHODS
    public void Init()
    {
        
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
