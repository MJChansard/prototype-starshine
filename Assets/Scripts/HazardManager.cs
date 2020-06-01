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

    private List<GameObject> hazards = new List<GameObject>();
    

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

        // Local variables
        int hazardType = Random.Range(0, hazardPrefabs.Length);
        int spawnAxis = Random.Range(1, 4);
        int spawnIndex;
        Vector2Int spawnPosition = new Vector2Int();

        // Spawn hazard & save reference to its <MovePattern>
        GameObject hazardToSpawn = Instantiate(hazardPrefabs[hazardType]);
        Debug.Log("Hazard to Spawn: " + hazardToSpawn);
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
        gm.PlaceObject(hazardToSpawn, spawnPosition);
        hazards.Add(hazardToSpawn);
    }


    public void RemoveHazard(GameObject hazard)
    {
        GridBlock gridBlock = gm.FindGridBlockContainingObject(hazard);
        gm.RemoveObject(hazard, gridBlock);
        hazards.Remove(hazard);
    }


    public void OnTickUpdate()
    { 
        for (int i = hazards.Count - 1; i > -1; i--)
        {
            GameObject hazard = hazards[i];

            Health hp = hazard.GetComponent<Health>();
            if (hp.CurrentHP <= 0)
            {
                RemoveHazard(hazard);
            }

            MovePattern move = hazard.GetComponent<MovePattern>();
            if (move.moveRate == 1 || currentTick % move.moveRate == 0)
            {
                Debug.Log(hazard.name + " is moving by " + move.delta);
                GridBlock gridBlock = gm.FindGridBlockContainingObject(hazard);
                if (gridBlock != null)
                {
                    bool successful = gm.CheckIfMoveIsValid(hazard, gridBlock.location, gridBlock.location + move.delta);
                    if (!successful)
                    {
                        RemoveHazard(hazard);
                    }
                    else
                    {
                        // Movement coroutine needs to go here
                        //Vector2Int[] parms = new Vector2Int[2] { gridBlock.location, gridBlock.location + move.delta };
                        //StartCoroutine(MoveHazardCoroutine(parms));

                        AnimateHazardMovement(hazard, gridBlock, move.delta);
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

        if (hazards.Count == 0)
        {
            Debug.Log("Preparing Hazard: Hazard count condition.");
            PrepareHazard();
        }

        currentTick++;
        Debug.LogFormat("Current tick till spawn: {0}", ticksUntilNewSpawn);
        ticksUntilNewSpawn--;
    }

    private void AnimateHazardMovement(GameObject hazard, Vector2Int currentGridLocation, Vector2Int moveDirection)
    {

    }

    private IEnumerator MoveHazardCoroutine(Vector2Int[] parms)
    { 
        Vector3 currentWorldLocation = gm.GridToWorld(parms[0]);
        Vector3 targetWorldLocation = gm.GridToWorld(parms[1]);

        float distance = Vector3.Distance(currentWorldLocation, targetWorldLocation);
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= 1.0f)
        {
            float traveled = (Time.time - startTime) * 1.0f;
            percentTraveled = traveled / distance;
            transform.position = Vector3.Lerp(currentWorldLocation, targetWorldLocation, Mathf.SmoothStep(0, 1, percentTraveled));

            yield return null;
        }

    }
}