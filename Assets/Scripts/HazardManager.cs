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

    private int currentTick;
    private int ticksUntilNewSpawn;
    private int minTicksUntilSpawn = 4;
    private int maxTicksUntilSpawn = 8;

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

    private List<GameObject> hazards = new List<GameObject>();
    
    void Start()
    {
        gm = GetComponent<GridManager>();      
        
        Debug.Log("gridWidth: " + gm.GridWidth);
        Debug.Log("gridHeight: " + gm.GridHeight);

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
    }

    public void Init()
    {
        UpdateSpawnLocations();
        PrepareHazard();
        currentTick = 1;
    }


    private void UpdateSpawnLocations()
    {
        int colRange = gm.GridWidth - 1;    // 10 - 1 = 9
        int rowRange = gm.GridHeight - 1;   // 8 - 1 = 7

        // Populate spawnMoveUp List and populate spawnMoveDown List
        for (int x = 1; x < colRange; x++)
        {
            if (gm.levelGrid[x, 0].canSpawn)
            {
                Debug.Log("Populating [spawnMoveUp].");
                spawnMoveUp.Add(gm.levelGrid[x, 0]);

            }

            if (gm.levelGrid[x, rowRange].canSpawn)
            {
                Debug.Log("Populating [spawnMoveDown].");
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
         */

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
                spawnMovement.SetMovePatternUp();

                Debug.Log("Selected spawnPosition: " + spawnPosition);
                break;
            case 2:
                spawnIndex = Random.Range(0, spawnMoveDown.Count);
                spawnPosition = spawnMoveDown[spawnIndex].location;
                spawnMovement.SetMovePatternDown();
                break;
            case 3:
                spawnIndex = Random.Range(0, spawnMoveLeft.Count);
                spawnPosition = spawnMoveLeft[spawnIndex].location;
                spawnMovement.SetMovePatternLeft();
                break;
            case 4:
                spawnIndex = Random.Range(0, spawnMoveRight.Count);
                spawnPosition = spawnMoveRight[spawnIndex].location;
                spawnMovement.SetMovePatternRight();
                break;
        }

        gm.PlaceObject(spawn, spawnPosition);
        hazards.Add(spawn);

    }


    public void MoveHazards(int currentTurn)
    {
        if (hazards.Count > 0)
        {
            List<GameObject> hazardsToRemove = new List<GameObject>();

            foreach (GameObject hazard in hazards)
            {
                MovePattern move = hazard.GetComponent<MovePattern>();
                if (move.moveRate == 1 || currentTurn % move.moveRate == 0)
                {
                    Debug.Log(hazard.name + " is moving by " + move.delta);
                    GridBlock gridBlock = gm.FindGridBlockContainingObject(hazard);
                    if (gridBlock != null)
                    {
                        bool successful = gm.RequestMove(hazard, gridBlock.location, gridBlock.location + move.delta);
                        if (!successful)
                        {
                            hazardsToRemove.Add(hazard);
                        }
                    }
                }
            }

            foreach (GameObject hazard in hazardsToRemove)
            {
                Debug.Log("Number of hazards prior to removal: " + hazards.Count);
                GridBlock gridBlock = gm.FindGridBlockContainingObject(hazard);
                gm.RemoveObject(hazard, gridBlock);
                
                Debug.Log("Index of out of bounds element: " + hazards.IndexOf(gameObject));
                hazards.Remove(hazard);
            }
        }
        else return;
    }


    public void OnTickUpdate()
    {
        currentTick++;
        ticksUntilNewSpawn--;

        if (ticksUntilNewSpawn == 0)
        {
            PrepareHazard();
            ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
        }

        if (hazards.Count == 0)
        {
            PrepareHazard();
        }
    }
}