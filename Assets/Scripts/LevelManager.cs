using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager
{
    [SerializeField] private LevelData levelData;
    [HideInInspector] public LevelData.LevelDataRow currentLevelData;

    private void GetLevelData(int levelNumber)
    {
        currentLevelData = levelData.LevelTable[levelNumber];
    }
}
