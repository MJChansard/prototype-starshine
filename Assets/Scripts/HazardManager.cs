using System.Collections;
using System.Collections.Generic;
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
        /*
        else    // Resolve collisions
        {
            GameObject objectOnTargetGridBlock = targetGridLocation.objectOnBlock;
            Hazard hazardOnTargetGridBlock = objectOnTargetGridBlock.GetComponent<Hazard>();

            if (hazardOnTargetGridBlock != null)
            {
                
                switch (hazardOnTargetGridBlock.HazardName)
                {
                    case "Small Asteroid":
                        Health hp = hazardOnTargetGridBlock.GetComponent<Health>();
                        break;

                    case "Large Asteroid":
                        break;

                    case "Missile":
                        break;
                }
            }
        }
        */
    }

    public void RemoveHazard(Hazard hazard)
    {
        GameObject hazardToRemove = hazard.gameObject;
        Vector2Int gridPosition = gm.FindGridBlockContainingObject(hazardToRemove).location;
        //gm.RemoveObject(hazardToRemove, gridBlock);
        gm.RemoveObjectFromGrid(hazardToRemove, gridPosition);
        hazardsInPlay.Remove(hazard);
    }




    public void OnTickUpdate()
    { 
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = hazardsInPlay[i].gameObject;
            Health hazardHealth = hazardObject.GetComponent<Health>();

            if (hazardHealth != null && hazardHealth.CurrentHP <= 0)
            {
                RemoveHazard(hazardsInPlay[i]);
                continue;
            }
            

            MovePattern move = hazardObject.GetComponent<MovePattern>();

            if (move.moveRate == 1 || currentTick % move.moveRate == 0)
            {
                Debug.Log(hazardObject.name + " is moving by " + move.delta);

                Vector2Int originGridPosition = gm.WorldToGrid(hazardsInPlay[i].currentWorldLocation);
                Vector2Int destinationGridPosition = originGridPosition + move.delta;

                GridBlock currentGridBlock = gm.FindGridBlockByLocation(originGridPosition);
                if (currentGridBlock != null)
                {
                    //bool moveIsValid = gm.CheckIfGridBlockInBounds(targetGridLocation) && gm.CheckIfGridBlockIsNotOccupied(targetGridLocation);
                    //bool collisionImminent = gm.CheckIfGridBlockInBounds(targetGridLocation) && gm.CheckIfGridBlockIsNotOccupied(targetGridBlock);

                    bool moveInBounds = gm.CheckIfGridBlockInBounds(destinationGridPosition);
                    bool collisionImminent = gm.CheckIfGridBlockIsOccupied(destinationGridPosition);

                    if (!moveInBounds)
                    {
                        RemoveHazard(hazardsInPlay[i]);
                    }
                    else if (!collisionImminent)
                    {
                        hazardsInPlay[i].targetWorldLocation = gm.GridToWorld(destinationGridPosition);
                        StartCoroutine(MoveHazardCoroutine(hazardsInPlay[i]));
                        //gm.UpdateGridPosition(hazardObject, currentGridLocation, targetGridLocation);
                        gm.AddObjectToGrid(hazardObject, destinationGridPosition);
                        gm.RemoveObjectFromGrid(hazardObject, originGridPosition);
                    }
                    // Tests for collisions need to go here
                    // Checks need to be separated
                    //  1. In bounds
                    //  2. Available for occupation
                    //  3. Collision
                    /*
                    else if (collisionImminent)
                    {
                        GameObject objectCollidedWith = gm.FindGridBlockByLocation(destinationGridPosition).objectOnBlock;
                        Hazard hazardCollidedWith = objectCollidedWith.GetComponent<Hazard>();
                        Health hazardCollidedWithHP = hazardCollidedWith.GetComponent<Health>();
                        
                        if (hazardCollidedWith != null)
                        {
                            switch (hazardsInPlay[i].HazardName)
                            {
                                case "Small Asteroid":
                                    switch(hazardCollidedWith.HazardName)
                                    {
                                        case "Small Asteroid":
                                            if (hazardCollidedWithHP != null)
                                            {
                                                hazardCollidedWithHP.ApplyDamage(100);
                                            }
                                            // TODO: Instantiate explosion
                                            RemoveHazard(hazardsInPlay[i]);
                                            break;
                                        
                                        case "Large Asteroid":
                                            if (hazardCollidedWithHP != null)
                                            {
                                                hazardCollidedWithHP.ApplyDamage(100);
                                            }
                                            // TODO: Instantiate explosion
                                            RemoveHazard(hazardsInPlay[i]);
                                            break;

                                        case "Missile":
                                            {
                                                hazardCollidedWithHP.ApplyDamage(1);
                                            }
                                            // TODO: Instantiate explosion
                                            RemoveHazard(hazardsInPlay[i]);
                                            break;
                                    }
                                    /*
                                    if (hazardCollidedWith.HazardName == "Small Asteroid")
                                    {
                                        RemoveHazard(hazardsInPlay[i]);
                                        //Instantiate explosion here
                                    }
                                    if (hazardCollidedWith.HazardName == "Missile")
                                    {

                                    }
                                    
                                    break;

                                case "Large Asteroid":
                                    if (hazardCollidedWith.HazardName == "Small Asteroid")
                                    {
                                        RemoveHazard(hazardCollidedWith);
                                        // Worried about this because we're altering the <List>
                                        // Maybe it would be better to set health to 0 so that element is removed at the appropriate 
                                        // time (ie. when that element is being processed in the for loop)
                                        //Instantiate explosion here
                                    }
                                    break;
                            }
                        
                    }
                    */
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