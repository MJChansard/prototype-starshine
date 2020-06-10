using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    #region Inspector Attributes
    [SerializeField]
    Hazard[] hazardPrefabs;
    #endregion

    #region Private Fields    
    private GridManager gm;

    private int currentTick = 0;
    private int ticksUntilNewSpawn;
    private int minTicksUntilSpawn = 4;
    private int maxTicksUntilSpawn = 8;
    
    private List<GridBlock> spawnMoveUp = new List<GridBlock>();
    private List<GridBlock> spawnMoveDown = new List<GridBlock>();
    private List<GridBlock> spawnMoveLeft = new List<GridBlock>();
    private List<GridBlock> spawnMoveRight = new List<GridBlock>();
    //private List<GridBlock> spawnMultiMove = new List<GridBlock>();
    #endregion

    private List<Hazard> hazardsInPlay = new List<Hazard>();
    

    private void Start()
    {
        gm = GetComponent<GridManager>(); 
        
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
         */

        Debug.Log("PrepareHazard() called.");

        // Local variables
        int hazardType = Random.Range(0, hazardPrefabs.Length);
        int spawnAxis = Random.Range(1, 4);
        int spawnIndex;
        Vector2Int spawnPosition = new Vector2Int();

        // Spawn hazard & store component references
        Hazard hazardToSpawn = Instantiate(hazardPrefabs[hazardType]) as Hazard;
        MovePattern spawnMovement = hazardToSpawn.GetComponent<MovePattern>();

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

        AddHazard(hazardToSpawn, spawnPosition);
    }


    public void AddHazard(Hazard hazard, Vector2Int position)
    {
        Debug.Log("AddHazard() called.");
        gm.PlaceObject(hazard.gameObject, position);

        hazard.currentWorldLocation = gm.GridToWorld(position);
        hazard.targetWorldLocation = gm.GridToWorld(position);

        hazardsInPlay.Add(hazard);
    }


    public void RemoveHazard(Hazard hazard)
    {
        GameObject hazardToRemove = hazard.gameObject;
        GridBlock gridBlock = gm.FindGridBlockContainingObject(hazardToRemove);
        gm.RemoveObject(hazardToRemove, gridBlock);
        hazardsInPlay.Remove(hazard);
    }




    public void OnTickUpdate()
    { 
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = hazardsInPlay[i].gameObject;
            Health hazardHealth = hazardObject.GetComponent<Health>();
            MovePattern move = hazardObject.GetComponent<MovePattern>();

            if (hazardHealth != null && hazardHealth.CurrentHP <= 0)
            {
                RemoveHazard(hazardsInPlay[i]);
                continue;
            }

            
            if (move.moveRate == 1 || currentTick % move.moveRate == 0)
            {
                Debug.Log(hazardObject.name + " is moving by " + move.delta);

                GridBlock currentGridBlock = gm.FindGridBlockContainingObject(hazardObject);
                Vector2Int targetGridLocation = currentGridBlock.location + move.delta;

                if (currentGridBlock != null)
                {
                    bool successful = gm.CheckIfMoveIsValid(hazardObject, currentGridBlock.location, currentGridBlock.location + move.delta);
                    if (!successful)
                    {
                        RemoveHazard(hazardsInPlay[i]);
                    }
                    else
                    {
                        hazardsInPlay[i].targetWorldLocation = gm.GridToWorld(targetGridLocation);
                        StartCoroutine(MoveHazardCoroutine(hazardsInPlay[i]));
                    }
                }
            }
        }

        if (ticksUntilNewSpawn == 0)
        {
            Debug.Log("Preparing Hazard: tick condition.");
            PrepareHazard();
            ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
        }

        if (hazardsInPlay.Count == 0)
        {
            Debug.Log("Preparing Hazard: Hazard count condition.");
            PrepareHazard();
        }

        currentTick++;
        Debug.LogFormat("Current tick till spawn: {0}", ticksUntilNewSpawn);
        ticksUntilNewSpawn--;
    }

    private IEnumerator MoveHazardCoroutine(Hazard hazardToMove)
    {
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= 1.0f)
        {
            float traveled = (Time.time - startTime) * 1.0f;
            percentTraveled = traveled / hazardToMove.Distance;
            hazardToMove.transform.position = Vector3.Lerp(hazardToMove.currentWorldLocation, hazardToMove.targetWorldLocation, Mathf.SmoothStep(0, 1, percentTraveled));

            yield return null;
        }

        hazardToMove.currentWorldLocation = hazardToMove.targetWorldLocation;
    }
}