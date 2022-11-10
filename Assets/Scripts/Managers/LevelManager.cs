using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class LevelManager : MonoBehaviour
{
    //  #INSPECTOR
    [BoxGroup("GENERAL SETTINGS", centerLabel: true)]
    [ShowInInspector][DisplayAsString] int currentLevel;
    [BoxGroup("GENERAL SETTINGS")][ShowInInspector]public bool overrideLevelData { get; private set; }

    [BoxGroup("LEVEL CONFIGURATION", centerLabel: true)]
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorGridWidth;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorGridHeight;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorJumpFuelAmount;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorPhenomenaSpawnCount;
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CREATE SINGLE LEVEL")][SerializeField] int inspectorStationSpawnCount;
    
    [ShowIf("overrideLevelData")][TitleGroup("LEVEL CONFIGURATION/CUSTOM LEVELS")]
    [ShowIf("overrideLevelData")][SerializeField] LevelRecord[] inspectorLevels;



    //  #PROPERTIES
    public int CurrentLevel { get { return currentLevel; } }
    public LevelRecord CurrentLevelData
    {
        get
        {
            if (allLevels[0] != null)
                return allLevels[0];
            else
                return allLevels[1];
        }
    }
    int levelIndex
    {
        get { return currentLevel - 1; }
    }


    //  #FIELDS
    [ShowInInspector] LevelRecord[] allLevels;
        
    
    void Awake()    
    {
        currentLevel = 1;

        Debug.Log("Finding [LevelRecord]s");
        string[] levels = AssetDatabase.FindAssets("LevelRecord_");
        Debug.LogFormat("Found {0} levels", levels.Length.ToString());

        allLevels = new LevelRecord[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(levels[i]);
            Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(LevelRecord));
            allLevels[i] = (LevelRecord)(asset as ScriptableObject);
        }
        Debug.LogFormat("Stored {0} [LevelRecord]s", allLevels.Length.ToString());
        

        if(overrideLevelData)
        {
            LevelRecord singleLevel = LevelRecord.CreateLevelRecord(
                inspectorGridWidth,
                inspectorGridHeight,
                inspectorJumpFuelAmount,
                inspectorPhenomenaSpawnCount,
                inspectorStationSpawnCount
            );
            
            allLevels[0] = singleLevel;
        }

        if (inspectorLevels != null && inspectorLevels.Length > 0)
        {
            for (int i = 0; i < inspectorLevels.Length; i++)
            {
                allLevels[i + 1] = inspectorLevels[i];
            }
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
