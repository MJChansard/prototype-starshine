using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    #region Inspector Attributes
    [SerializeField]
    GameObject[] hazardPrefabs;
    #endregion

    #region Private Fields
    private GridManager gm;

    private int gridCornerLL;
    private int gridCornerLR;
    private int gridCornerUL;
    private int gridCornerUR;

    private int gridMinX;
    private int gridMaxX;
    private int gridMinY;
    private int gridMaxY;
    
    private List<GridBlock> spawnMoveUp = new List<GridBlock>();
    private List<GridBlock> spawnMoveDown = new List<GridBlock>();
    private List<GridBlock> spawnMoveLeft = new List<GridBlock>();
    private List<GridBlock> spawnMoveRight = new List<GridBlock>();
    //private List<GridBlock> spawnMultiMove = new List<GridBlock>();
    #endregion

    void Start()
    {
        gm = GetComponent<GridManager>();

        Debug.Log("gridWidth: " + gm.gridWidth);
        Debug.Log("gridHeight: " + gm.gridHeight);

        UpdateSpawnLocations();
        PrepareHazard();

        //GameObject asteroid = Instantiate(hazardPrefabs[0]);
        //gm.PlaceObject(asteroid, new Vector2Int(6, 6));
        //gm.hazards.Add(asteroid);
    }

    void Update()
    {
        
    }

    private void UpdateSpawnLocations()
    {
        int colRange = gm.gridWidth - 1;    // 10 - 1 = 9
        int rowRange = gm.gridHeight - 1;   // 8 - 1 = 7

        // Populate spawnMoveUp List and populate spawnMoveDown List
        for (int x = 1; x < colRange; x++)
        {
            if (gm.levelGrid[x, 0].canSpawn)
            {
                spawnMoveUp.Add(gm.levelGrid[x, 0]);

            }

            if (gm.levelGrid[x, rowRange].canSpawn)
            {
                spawnMoveDown.Add(gm.levelGrid[x, rowRange]);
            }
        }

        // Populate spawnMoveRight List and populate spawnMoveLeft List
        for (int y = 1; y < rowRange; y++)
        {
            if (gm.levelGrid[0, y].canSpawn)
            {
                spawnMoveRight.Add(gm.levelGrid[0, y]);
                
            }

            if(gm.levelGrid[colRange, y].canSpawn)
            {
                spawnMoveLeft.Add(gm.levelGrid[colRange, y]);
            }
        }
    }

    private void PrepareHazard()
    {
        /*  SUMMARY
         *   - Randomly select a spawn location
         *   - Randomly select a hazard to spawn
         *   - Activate hazard spawn movement pattern based on spawn location
         * * * */

        // Local variables
        int hazardType = Random.Range(0, hazardPrefabs.Length);
        int spawnAxis = Random.Range(1, 4);
        int spawnIndex;
        Vector2Int spawnPosition = new Vector2Int();

        // Spawn hazard & save reference to its <MovePattern>
        GameObject spawn = Instantiate(hazardPrefabs[hazardType]);
        Debug.Log("Hazard to Spawn: " + spawn);
        MovePattern spawnMovement = spawn.GetComponent<MovePattern>();

        switch (spawnAxis)
        {
            case 1:
                spawnIndex = Random.Range(0, spawnMoveUp.Count);
                spawnPosition = spawnMoveUp[spawnIndex].location;
                spawnMovement.SetMovePatternUp(spawnMovement.moveRate);

                Debug.Log("Selected spawnPosition: " + spawnPosition);
                break;
            case 2:
                spawnIndex = Random.Range(0, spawnMoveDown.Count);
                spawnPosition = spawnMoveDown[spawnIndex].location;
                spawnMovement.SetMovePatternDown(spawnMovement.moveRate);
                break;
            case 3:
                spawnIndex = Random.Range(0, spawnMoveLeft.Count);
                spawnPosition = spawnMoveLeft[spawnIndex].location;
                spawnMovement.SetMovePatternLeft(spawnMovement.moveRate);
                break;
            case 4:
                spawnIndex = Random.Range(0, spawnMoveRight.Count);
                spawnPosition = spawnMoveRight[spawnIndex].location;
                spawnMovement.SetMovePatternRight(spawnMovement.moveRate);
                break;
        }
        Debug.Log("Selected spawnPosition: " + spawnPosition);
        Debug.Log("Selected hazard move rate: " + spawnMovement.moveRate);
        Debug.Log("Selected hazard move delta: " + spawnMovement.delta);

        gm.PlaceObject(spawn, spawnPosition);
        gm.hazards.Add(spawn);

        // TODO
        // All spawned hazard currently progress down the grid 
        // I need to update MovePattern based on the starting GridBlock
    }

}