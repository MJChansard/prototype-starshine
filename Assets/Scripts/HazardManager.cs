using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    #region Inspector Attributes
    [SerializeField] Hazard[] hazardPrefabs;
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

        Debug.Log("Spawn Locations successfully updated.");
    }

 /*  SUMMARY
 *   - Randomly select a spawn location
 *   - Randomly select a hazard to spawn
 *   - Activate hazard spawn movement pattern based on spawn location
 */
    private void PrepareHazard()
    {
        Debug.Log("HazardManager.PrepareHazard() called.");

        int hazardType = Random.Range(0, hazardPrefabs.Length - 1);
        int spawnAxis = Random.Range(1, 4);
        int spawnIndex;
        Vector2Int spawnPosition = new Vector2Int();

        // Spawn hazard & store component references
        Hazard hazardToSpawn = Instantiate(hazardPrefabs[hazardType]);
        MovePattern spawnMovement = hazardToSpawn.GetComponent<MovePattern>();

        switch (spawnAxis)
        {
            case 1:
                spawnIndex = Random.Range(0, spawnMoveUp.Count);
                spawnPosition = spawnMoveUp[spawnIndex].location;
                spawnMovement.SetMovePatternUp();
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


    public void AddHazard(Hazard hazard, Vector2Int gridPosition, bool placeOnGrid = true)
    {
        Debug.Log("HazardManager.AddHazard() called.");

        GridBlock destinationGridPosition = gm.FindGridBlockByLocation(gridPosition);
        Vector3 worldLocation = gm.GridToWorld(gridPosition);

        if(destinationGridPosition.IsOccupied && placeOnGrid == false)
        {
            hazard.currentWorldLocation = worldLocation;
            hazard.targetWorldLocation = worldLocation;            

            hazardsInPlay.Add(hazard);
        }
        else if(!destinationGridPosition.IsOccupied)
        {
            hazard.transform.position = worldLocation;
            gm.AddObjectToGrid(hazard.gameObject, gridPosition);

            hazard.currentWorldLocation = worldLocation;
            hazard.targetWorldLocation = worldLocation;

            hazardsInPlay.Add(hazard);
        }
    }

    public void RemoveHazard(Hazard hazard)
    {
        GameObject hazardToRemove = hazard.gameObject;
        Vector2Int gridPosition = gm.FindGridBlockContainingObject(hazardToRemove).location;
        
        gm.RemoveObjectFromGrid(hazardToRemove, gridPosition);
        hazardsInPlay.Remove(hazard);
    }


    public float OnTickUpdate()
    {
        float delayTime = 2.0f;

        List<GridBlock> allPossibleBlockCollisions = new List<GridBlock>();

        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = hazardsInPlay[i].gameObject;
            Health hazardHealth = hazardObject.GetComponent<Health>();

            if (hazardHealth != null && hazardHealth.CurrentHP <= 0)
            {
                StartCoroutine(DestroyHazardCoroutine(hazardsInPlay[i]));
                RemoveHazard(hazardsInPlay[i]);
                continue;
            }

            MovePattern move = hazardObject.GetComponent<MovePattern>();

            if (currentTick % move.moveRate == 0)
            {
                Debug.Log(hazardsInPlay[i].HazardName + " is moving by " + move.delta);

                Vector2Int originGridPosition = gm.WorldToGrid(hazardsInPlay[i].currentWorldLocation);
                Vector2Int destinationGridPosition = originGridPosition + move.delta;

                bool moveInBounds = gm.CheckIfGridBlockInBounds(destinationGridPosition);
                bool collisionImminent = gm.CheckIfGridBlockIsOccupied(destinationGridPosition);

                if (!moveInBounds)
                {
                    StartCoroutine(DestroyHazardCoroutine(hazardsInPlay[i], 2.0f));
                    RemoveHazard(hazardsInPlay[i]);
                }
                else
                {
                    gm.RemoveObjectFromGrid(hazardObject, originGridPosition);
                    Debug.Log("Removing " + hazardsInPlay[i].HazardName + " from " + originGridPosition.ToString());
                    
                    gm.AddObjectToGrid(hazardObject, destinationGridPosition);
                    Debug.Log("Adding " + hazardsInPlay[i].HazardName + " to " + destinationGridPosition.ToString());

                    hazardsInPlay[i].targetWorldLocation = gm.GridToWorld(destinationGridPosition);                        
                    StartCoroutine(MoveHazardCoroutine(hazardsInPlay[i]));

                    allPossibleBlockCollisions.Add(gm.FindGridBlockByLocation(destinationGridPosition));

                    /*
                        if (collisionImminent)
                        {
                            GridBlock destinationGridBlock = gm.FindGridBlockByLocation(destinationGridPosition);
                            collisionsToProcess.Add(destinationGridBlock);
                        }
                    */
                }                
            }

            // if (i > -1) Debug.Log("Movement for " + hazardsInPlay[i].HazardName + " completed.");
        
        }

        List<GridBlock> uniqueListOfCollisions = allPossibleBlockCollisions.Distinct().ToList();
        /*
        foreach(GridBlock block in uniqueListOfCollisions)
        {
            if (block.objectsOnBlock.Count > 1)
            {
                foreach (GameObject gameObject in block.objectsOnBlock)
                {
                    Hazard hazardObject = gameObject.GetComponent<Hazard>();
                    Debug.Log("On " + block.location.ToString() + ": " + hazardObject.HazardName);
                }
            }
        }
        */

        foreach (GridBlock gridBlock in uniqueListOfCollisions)
        {
            Debug.Log("Processing collision on " + gridBlock.location.ToString());
            // Iterate through collisionsToProcess
            for (int i = 0; i < gridBlock.objectsOnBlock.Count; i++)
            {
                GameObject gameObject = gridBlock.objectsOnBlock[i];
                Health gameObjectHealth = gameObject.GetComponent<Health>();
                ContactDamage gameObjectDamage = gameObject.GetComponent<ContactDamage>();

                for (int j = 1 + i; j < gridBlock.objectsOnBlock.Count; j++)
                {                    
                    GameObject otherGameObject = gridBlock.objectsOnBlock[j];
                    Health otherGameObjectHealth = otherGameObject.GetComponent<Health>();
                    ContactDamage otherGameObjectDamage = otherGameObject.GetComponent<ContactDamage>();

                    if (gameObjectDamage != null && otherGameObjectHealth != null)
                    {
                        Debug.Log("Subtracting " + gameObjectDamage.DamageAmount.ToString() + " from " + otherGameObject.name);
                        otherGameObjectHealth.SubtractHealth(gameObjectDamage.DamageAmount);
                    }

                    if (gameObjectHealth != null && otherGameObjectDamage != null)
                    {
                        Debug.Log("Subtracting " + otherGameObjectDamage.DamageAmount.ToString() + " from " + gameObject.name);
                        gameObjectHealth.SubtractHealth(otherGameObjectDamage.DamageAmount);
                    }
                }
            }
        }

        //TODO: CheckHazardHealth()
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = hazardsInPlay[i].gameObject;
            Health hazardHealth = hazardObject.GetComponent<Health>();

            if (hazardHealth != null && hazardHealth.CurrentHP <= 0)
            {
                StartCoroutine(DestroyHazardCoroutine(hazardsInPlay[i], 2.0f));
                RemoveHazard(hazardsInPlay[i]);
            }

        }

        // Spawn stuff
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
        
        return delayTime;
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

    private IEnumerator DestroyHazardCoroutine(Hazard hazardToDestroy, float delay = 0.0f)
    {
        Debug.Log("DestroyHazardCoroutine() called.");
        yield return new WaitForSeconds(delay);
        Destroy(hazardToDestroy.gameObject);
        Debug.Log("DestroyHazardCoroutine() ended.");
        
        //TODO: Spawn explosion here
    }
}