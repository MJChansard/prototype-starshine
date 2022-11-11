using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

/*  NOTES
 *   -  Considering removing this class as a lot of its functionality has been replaced with the 
 *      LevelRecord scriptable objects.
 *      
 *   -  What could be cool though is this class serve lots of debugging functionality
 *      ~ Button to load the next or previous level
 * 
 */
public class LevelManager : MonoBehaviour
{
    //  #INSPECTOR
    [ShowInInspector][DisplayAsString] int currentLevel;
    [SerializeField] bool verboseLogging;
    [ShowInInspector] LevelRecord[] allLevels;

    //  #PROPERTIES
    public int CurrentLevel { get { return currentLevel; } }
    public LevelRecord CurrentLevelData
    {
        get
        {
            LevelRecord level = allLevels[levelIndex];
            if (level != null) 
            {
                return level;
            }
            else
            {
                Debug.LogFormat("No [LevelRecord] found for the current level.");
                return null;
            }
        }
    }
    int levelIndex
    {
        get { return currentLevel - 1; }
    }


    void Awake()    
    {
        currentLevel = 1;

        if (verboseLogging)
            Debug.Log("Finding [LevelRecord]s");

        string[] levelGUIDs = AssetDatabase.FindAssets("LevelRecord_");
        
        if (verboseLogging)
            Debug.LogFormat("Found {0} levels", levelGUIDs.Length.ToString());

        allLevels = new LevelRecord[levelGUIDs.Length];
        for (int i = 0; i < levelGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(levelGUIDs[i]);
            Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(LevelRecord));
            allLevels[i] = (LevelRecord)(asset as ScriptableObject);
        }

        if (verboseLogging)
            Debug.LogFormat("Stored {0} [LevelRecord]s", allLevels.Length.ToString());
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
}
