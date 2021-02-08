using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridObjectManager : MonoBehaviour
{
    #region Grid Object Manager Data   
    [SerializeField] private bool VerboseConsole = true;

    private GridManager gm;
    private PlayerManager pm;

    private int currentTick = 0;

    private Vector2Int minVector2;
    private Vector2Int maxVector2;

    private List<GridObject> gridObjectsInPlay = new List<GridObject>();
    private List<GridBlock> uniquePossibleBlockCollisions = new List<GridBlock>();
    #endregion


    #region Object Spawning
    [Header("Spawn Management")]
    [SerializeField] private int minTicksUntilSpawn = 2;
    [SerializeField] private int maxTicksUntilSpawn = 4;

    [HideInInspector] public List<SpawnSequence> insertSpawnSequences = new List<SpawnSequence>();
    private Queue<SpawnStep> spawnQueue = new Queue<SpawnStep>();
    private int ticksUntilNewSpawn;

    [Header("Grid Object Inventory")]
    [SerializeField] private GridObject[] gridObjectPrefabs;
    #endregion


    private void Start()
    {
        gm = GetComponent<GridManager>();

        minVector2 = new Vector2Int(gm.BoundaryLeftActual, gm.BoundaryBottomActual);
        maxVector2 = new Vector2Int(gm.BoundaryRightActual, gm.BoundaryTopActual);

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);

        if (VerboseConsole)
        {
            Debug.LogFormat("Length of uniquePossibleBlockCollisions: {0}", uniquePossibleBlockCollisions.Count);
        }
    }

    public void Init()
    {
        currentTick = 1;
        pm = GameObject.FindWithTag("Player").GetComponent<PlayerManager>();

        if (insertSpawnSequences.Count > 0)
        {
            for (int i = 0; i < insertSpawnSequences.Count; i++)
            {
                for (int j = 0; j < insertSpawnSequences[i].hazardSpawnSteps.Length; j++)
                {
                    spawnQueue.Enqueue(insertSpawnSequences[i].hazardSpawnSteps[j]);
                }
            }

            CreateGridObject(spawnQueue.Dequeue());
        }
        else
        {
            AddSpawnStep();
            CreateGridObject(spawnQueue.Dequeue());
        }

    }

    // Move to Grid Manager
    // Move generic spawn rule processing stuff into Grid Manager
    // Hey GM, give me a list of all generic spawn locations
    // Then HM further wittles down the list based on hazard specific criteria
    // HM.GetLocationsInHazardPaths()


    private void AddSpawnStep()
    {
        /*	SUMMARY
         *	-   Randomly selects a Hazard
         *  -   Randomly selects a spawn location
         *  -   Enqueue (new SpawnStep instance)
         */

        if (VerboseConsole) Debug.Log("HazardManager.CreateSpawnStep() called.");

        int gridObjectSelector = Random.Range(0, gridObjectPrefabs.Length);
        Vector2Int hazardSpawnLocation = new Vector2Int();

        // DEBUG: Make sure spawn selection is working appropriately
        if (VerboseConsole) Debug.LogFormat("Array Length: {0}, Random value: {1}", gridObjectPrefabs.Length, gridObjectSelector);

        // Identify an appropriate spawn location
        List<Vector2Int> availableSpawns = gm.GetSpawnLocations(gridObjectPrefabs[gridObjectSelector].spawnRules);
        Vector2Int targetLocation = availableSpawns[Random.Range(0, availableSpawns.Count)];
        hazardSpawnLocation.Set(targetLocation.x, targetLocation.y);

        // Identify what to spawn
        /*
        GridObject identifyObject = gridObjectPrefabs[gridObjectSelector];
        if (identifyObject.GetType() == typeof(Hazard))
        {

        }
        */

        // Create the SpawnStep
        SpawnStep newSpawnStep = ScriptableObject.CreateInstance<SpawnStep>();
        newSpawnStep.Init(gridObjectPrefabs[gridObjectSelector], hazardSpawnLocation);
        spawnQueue.Enqueue(newSpawnStep);
    }

    private void CreateGridObject(SpawnStep spawnStep)
    {
        /*  SUMMARY
        *   - Process SpawnStep data to identify hazard to spawn and spawn location
        *   - Instantiate hazard
        *   - Prepare hazard for gameplay
        *       ~ Set Animation Mode
        *       ~ Toggle Invincibility
        *       ~ Activates Rotator
        *       ~ Sets MovePattern
        */

        if (VerboseConsole) Debug.Log("GridObjectManager.CreateHazard() called.");

        // Question Pat about this part
        /*
        int hazardIndex = 0;
        for (int i = 0; i < gridObjectPrefabs.Length; i++)
        {
            if (gridObjectPrefabs[i].hazardType == spawnStep.HazardType)
            {
                hazardIndex = i;
                break;
            }
        }
        */

        //Hazard hazardToSpawn = Instantiate(gridObjectPrefabs[hazardIndex]);
        GridObject newSpawn = Instantiate(spawnStep.gridObject);

        string borderName = "";
        if (spawnStep.SpawnLocation.y == gm.BoundaryBottomActual) borderName = "Bottom";
        else if (spawnStep.SpawnLocation.y == gm.BoundaryTopActual) borderName = "Top";
        else if (spawnStep.SpawnLocation.x == gm.BoundaryRightActual) borderName = "Right";
        else if (spawnStep.SpawnLocation.x == gm.BoundaryLeftActual) borderName = "Left";

        newSpawn.Init(borderName);

        //Debug line
        //Hazard hazardToSpawn = Instantiate(hazardPrefabs[1]);

        //hazardToSpawn.SetHazardAnimationMode(Hazard.HazardMode.Spawn, hazardToSpawn.hazardType);
        //hazardToSpawn.GetComponent<Health>().ToggleInvincibility(true);

        /*
        newSpawn.SetAnimationMode(GridObject.Mode.Spawn);
        Health newSpawnHealth = newSpawn.GetComponent<Health>();
        if (newSpawnHealth != null) newSpawnHealth.ToggleInvincibility(true);

        MovePattern newSpawnMove = newSpawn.GetComponent<MovePattern>();
        Rotator newSpawnRotator = newSpawn.GetComponent<Rotator>();

        if(newSpawnMove != null && newSpawnRotator != null)
        {
            switch (borderName)
            {
                case "Bottom":
                    newSpawnMove.SetMovePatternUp();
                    newSpawnRotator.ApplyRotation(newSpawn.hazardType, borderName);
                    break;

                case "Top":
                    if (newSpawn.spawnRules.requiresOrientation)
                    {
                        newSpawn.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                        newSpawn.spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                    }

                    newSpawnMove.SetMovePatternDown();
                    newSpawnRotator.ApplyRotation(newSpawn.hazardType, borderName);
                    break;

                case "Right":
                    if (newSpawn.spawnRules.requiresOrientation)
                    {
                        newSpawn.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                        newSpawn.spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                    }

                    newSpawnMove.SetMovePatternLeft();
                    newSpawnRotator.ApplyRotation(newSpawn.hazardType, borderName);
                    break;

                case "Left":
                    if (newSpawn.spawnRules.requiresOrientation)
                    {
                        newSpawn.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                        newSpawn.spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                    }

                    newSpawnMove.SetMovePatternRight();
                    newSpawnRotator.ApplyRotation(newSpawn.hazardType, borderName);
                    break;
            }
        }
        */
        
        AddGridObject(newSpawn, spawnStep.SpawnLocation);

        if (VerboseConsole) Debug.Log("GridObjectManager.CreateGridObject() completed.");
    }

    public void AddGridObject(GridObject gridObject, Vector2Int gridLocation, bool placeOnGrid = true)
    {
        Debug.Log("GridObjectManager.AddHazard() called.");

        //GridBlock destinationGridPosition = gm.FindGridBlockByLocation(gridLocation);
        Vector3 worldLocation = gm.GridToWorld(gridLocation);

        //if(destinationGridPosition.IsAvailableForPlayer && placeOnGrid == false)
        if (placeOnGrid == false)
        {
            gridObject.currentWorldLocation = worldLocation;
            gridObject.targetWorldLocation = worldLocation;

            gridObjectsInPlay.Add(gridObject);
        }
        //else if(!destinationGridPosition.IsAvailableForPlayer)
        else
        {
            gridObject.transform.position = worldLocation;
            gm.AddObjectToGrid(gridObject.gameObject, gridLocation);

            gridObject.currentWorldLocation = worldLocation;
            gridObject.targetWorldLocation = worldLocation;

            gridObjectsInPlay.Add(gridObject);
        }
    }

    public void RemoveGridObjectFromPlay(GridObject objectToRemove, bool removeFromGrid = true)
    {
        GameObject gameObjectToRemove = objectToRemove.gameObject;
        Vector2Int gridPosition = gm.FindGridBlockContainingObject(gameObjectToRemove).location;

        if (removeFromGrid) gm.RemoveObjectFromGrid(gameObjectToRemove, gridPosition);
        gridObjectsInPlay.Remove(objectToRemove);
    }


    private bool CheckHazardHasHealth(GameObject hazardObject)
    {
        // This might better be suited as a Property in Health.cs

        Health hazardHealth = hazardObject.GetComponent<Health>();
        if (hazardHealth != null && hazardHealth.CurrentHP > 0)
        {
            return true;
        }
        return false;
    }


    private IEnumerator HazardDropLoot(GridObject gridObject, Vector3 dropLocation, float delayAppear = 1.0f)
    {
        LootHandler lh = gridObject.gameObject.GetComponent<LootHandler>();
        if (lh != null)
        {
            GameObject lootObjectToDrop = lh.RequestLootDrop(dropLocation, forced: true);

            if (lootObjectToDrop != null)
            {
                MeshRenderer renderer = lootObjectToDrop.GetComponent<MeshRenderer>();
                renderer.enabled = false;

                Vector2Int dropGridLocation = gm.WorldToGrid(dropLocation);
                gm.AddObjectToGrid(lootObjectToDrop, dropGridLocation);
                lootObjectToDrop.GetComponent<Rotator>().enabled = true;

                yield return new WaitForSeconds(delayAppear);
                renderer.enabled = true;
            }
        }
    }


    public float OnTickUpdate()
    {
        /*  STEPS
         * 
         *  1) GridObject Health Check
         *  2) Move hazards
         *  3) Detect Fly-Bys
         *  4) Hazard Health Check
         *  5) Collisions on a GridBlock
         */

        #region Hazard Tick Duration
        bool moveOccurredThisTick = false;
        float moveDurationSeconds = 1.0f;

        bool hazardDestroyedThisTick = false;
        float destroyDurationSeconds = 2.0f;

        float delayTime = 0.0f;
        #endregion

        // Player collision check
        if (uniquePossibleBlockCollisions.Count > 0) ProcessCollisionsOnGridBlock(ref uniquePossibleBlockCollisions);

        // Hazard Health Check - Check for hazards destroyed during Player turn
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            if (!CheckHazardHasHealth(gridObjectsInPlay[i].gameObject))
            {
                StartCoroutine(DestroyHazardCoroutine(gridObjectsInPlay[i]));

                // Current location for processing damage during Player phase
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].currentWorldLocation); 
                StartCoroutine(HazardDropLoot(gridObjectsInPlay[i], gridObjectsInPlay[i].currentWorldLocation, 0.0f));

                RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
            }
        }

        // Movement and Collision collections
        List<GridBlock> allPossibleBlockCollisions = new List<GridBlock>();
        Vector2Int[] allOriginGridLocations = new Vector2Int[gridObjectsInPlay.Count];
        Vector2Int[] allDestinationGridLocations = new Vector2Int[gridObjectsInPlay.Count];

        // Manage Movement Data
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = gridObjectsInPlay[i].gameObject;
            MovePattern move = hazardObject.GetComponent<MovePattern>();
            move.OnTickUpdate();

            Vector2Int originGridLocation = gm.WorldToGrid(gridObjectsInPlay[i].currentWorldLocation);
            allOriginGridLocations[i] = originGridLocation;

            if (move.CanMoveThisTurn())
            {
                Debug.Log(gridObjectsInPlay[i].ToString() + " is moving by " + move.delta);

                Vector2Int destinationGridLocation = originGridLocation + move.delta;
                allDestinationGridLocations[i] = destinationGridLocation;

                bool moveInBounds = gm.CheckIfGridBlockInBounds(destinationGridLocation);

                if (!moveInBounds)
                {
                    StartCoroutine(DestroyHazardCoroutine(gridObjectsInPlay[i], 2.0f));
                    RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
                    hazardDestroyedThisTick = true;
                }
                else
                {
                    gm.RemoveObjectFromGrid(hazardObject, originGridLocation);
                    Debug.Log("Removing " + gridObjectsInPlay[i].ToString() + " from " + originGridLocation.ToString());

                    gm.AddObjectToGrid(hazardObject, destinationGridLocation);
                    Debug.Log("Adding " + gridObjectsInPlay[i].ToString() + " to " + destinationGridLocation.ToString());

                    // Handle spawning cases
                    hazardObject.GetComponent<Health>().ToggleInvincibility(false);
                    //gridObjectsInPlay[i].SetHazardAnimationMode(Hazard.HazardMode.Play, gridObjectsInPlay[i].hazardType);
                    gridObjectsInPlay[i].SetAnimationMode(GridObject.Mode.Play);

                    Rotator hazardRotator = gridObjectsInPlay[i].GetComponent<Rotator>();
                    if (hazardRotator != null) hazardRotator.enabled = true;

                    gridObjectsInPlay[i].targetWorldLocation = gm.GridToWorld(destinationGridLocation);

                    allPossibleBlockCollisions.Add(gm.FindGridBlockByLocation(destinationGridLocation));
                    //InsertGridBlockCollision(destinationGridLocation);    -- Try this!
                }
            }
            else
            {
                gridObjectsInPlay[i].targetWorldLocation = gridObjectsInPlay[i].currentWorldLocation;
                allOriginGridLocations[i] = originGridLocation;
                allDestinationGridLocations[i] = originGridLocation;
            }
        }

        // Fly-By detection
        Debug.Log("Fly-By detection starting.");
        for (int i = 0; i < allOriginGridLocations.Length; i++)
        {
            for (int j = 0; j < allDestinationGridLocations.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                if (allOriginGridLocations[i] == allDestinationGridLocations[j] && allOriginGridLocations[j] == allDestinationGridLocations[i])
                {
                    // HazardsInPlay[i] and HazardsInPlay[j] are the Fly-By colliders
                    //Debug.LogFormat("Fly-By Object 1: {0}, Fly-By Object 2: {1}", hazardsInPlay[i], hazardsInPlay[j]);

                    GridObject flyByHazard1 = gridObjectsInPlay[i];
                    GridObject flyByHazard2 = gridObjectsInPlay[j];

                    Health flyByHazard1HP = flyByHazard1.gameObject.GetComponent<Health>();
                    flyByHazard1HP.SubtractHealth(flyByHazard2.GetComponent<ContactDamage>().DamageAmount);

                    Health flyByHazard2HP = flyByHazard2.gameObject.GetComponent<Health>();
                    flyByHazard2HP.SubtractHealth(flyByHazard1.GetComponent<ContactDamage>().DamageAmount);

                    //if (flyByHazard1.HazardName == "Missile" || flyByHazard2.HazardName == "Missile")
                    /*
                    if (flyByHazard1.HazardType == Hazard.Type.PlayerMissile ||
                        flyByHazard2.HazardType == Hazard.Type.PlayerMissile)
                    {
                        if (flyByHazard1.HazardType == Hazard.Type.PlayerMissile) StartCoroutine(MoveHazardCoroutine(flyByHazard2, 1.0f));
                        if (flyByHazard2.HazardType == Hazard.Type.PlayerMissile) StartCoroutine(MoveHazardCoroutine(flyByHazard1, 1.0f));
                    }
                    else*/
                    if (!CheckHazardHasHealth(flyByHazard1.gameObject) && !CheckHazardHasHealth(flyByHazard2.gameObject))
                    {
                        StartCoroutine(MoveHazardCoroutine(flyByHazard1, 1.0f));
                    }
                    else if (!CheckHazardHasHealth(flyByHazard1.gameObject))
                    {
                        // If flyByHazard1 did not survive ...
                        StartCoroutine(MoveHazardCoroutine(flyByHazard1, 1.0f));
                    }
                    else if (!CheckHazardHasHealth(flyByHazard2.gameObject))
                    {
                        // If flyByHazard2 did not survive...
                        StartCoroutine(MoveHazardCoroutine(flyByHazard2, 1.0f));
                    }
                }
            }
        }

        // Hazard Health check following Fly-By processing
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = gridObjectsInPlay[i].gameObject;

            if (!CheckHazardHasHealth(hazardObject))
            {
                StartCoroutine(DestroyHazardCoroutine(gridObjectsInPlay[i], 2.0f));
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation); // Target location for Fly-By cases
                StartCoroutine(HazardDropLoot(gridObjectsInPlay[i], gridObjectsInPlay[i].targetWorldLocation));
                RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
                hazardDestroyedThisTick = true;
            }
        }

        // Move hazards
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            if (gridObjectsInPlay[i].currentWorldLocation != gridObjectsInPlay[i].targetWorldLocation)
            {
                StartCoroutine(MoveHazardCoroutine(gridObjectsInPlay[i]));
                moveOccurredThisTick = true;
            }
        }

        // GridBlock Collisions
        uniquePossibleBlockCollisions = allPossibleBlockCollisions.Distinct().ToList();
        ProcessCollisionsOnGridBlock(ref uniquePossibleBlockCollisions);

        // Hazard Health Check following Hazard movement
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = gridObjectsInPlay[i].gameObject;

            if (!CheckHazardHasHealth(hazardObject))
            {
                StartCoroutine(DestroyHazardCoroutine(gridObjectsInPlay[i], 1.0f));
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation); // Drop loot where hazard will end up after MoveHazardCoroutine();
                StartCoroutine(HazardDropLoot(gridObjectsInPlay[i], gridObjectsInPlay[i].targetWorldLocation));
                RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
                hazardDestroyedThisTick = true;
            }
        }

        // Spawn stuff
        if (ticksUntilNewSpawn == 0 || gridObjectsInPlay.Count == 0)
        {
            if (spawnQueue.Count > 0)
            {
                CreateGridObject(spawnQueue.Dequeue());
            }
            else
            {
                AddSpawnStep();
                CreateGridObject(spawnQueue.Dequeue());
            }

            ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
        }

        currentTick++;
        ticksUntilNewSpawn--;

        if (moveOccurredThisTick) delayTime += moveDurationSeconds;
        if (hazardDestroyedThisTick) delayTime += destroyDurationSeconds;
        return delayTime;
    }

    private void ProcessCollisionsOnGridBlock(ref List<GridBlock> gridBlocks)
    {
        foreach (GridBlock gridBlock in gridBlocks)
        {
            if (gridBlock.objectsOnBlock.Count > 1)
            {
                Debug.Log("Processing collision on " + gridBlock.location.ToString());

                for (int i = 0; i < gridBlock.objectsOnBlock.Count; i++)
                {
                    GameObject gameObject = gridBlock.objectsOnBlock[i];
                    //Hazard hazard = gameObject.GetComponent<Hazard>();
                    GridObject gridObject = gameObject.GetComponent<GridObject>();

                    LootData loot = gameObject.GetComponent<LootData>();
                    Health gameObjectHealth = gameObject.GetComponent<Health>();
                    ContactDamage gameObjectDamage = gameObject.GetComponent<ContactDamage>();

                    for (int j = 1 + i; j < gridBlock.objectsOnBlock.Count; j++)
                    {
                        GameObject otherGameObject = gridBlock.objectsOnBlock[j];
                        //Hazard otherHazard = otherGameObject.GetComponent<Hazard>();
                        GridObject othergridObject = otherGameObject.GetComponent<GridObject>();
                        LootData otherLoot = otherGameObject.GetComponent<LootData>();
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

                        if (gameObject.CompareTag("Player") && otherLoot != null)
                        {
                            pm.AcceptLoot(otherLoot.Type, otherLoot.LootAmount);
                            otherGameObjectHealth.SubtractHealth(otherGameObjectHealth.CurrentHP);
                        }

                        if (loot != null && otherGameObject.CompareTag("Player"))
                        {
                            pm.AcceptLoot(loot.Type, loot.LootAmount);
                            gameObjectHealth.SubtractHealth(gameObjectHealth.CurrentHP);
                        }
                    }
                }
            }
        }

        gridBlocks.Clear();
    }

    public void InsertGridBlockCollision(Vector2Int gridLocation)
    {
        for (int i = 0; i < uniquePossibleBlockCollisions.Count; i++)
        {
            if (uniquePossibleBlockCollisions[i].location != gridLocation)
            {
                continue;
            }
            else
            {
                Debug.Log("Location already in Collision List.");
                break;
            }
        }

        uniquePossibleBlockCollisions.Add(gm.FindGridBlockByLocation(gridLocation));
    }



    private IEnumerator MoveHazardCoroutine(GridObject objectToMove, float hazardTravelLength = 1.0f)
    {
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= hazardTravelLength)
        {
            float traveled = (Time.time - startTime) * 1.0f;
            percentTraveled = traveled / objectToMove.Distance;
            objectToMove.transform.position = Vector3.Lerp(objectToMove.currentWorldLocation, objectToMove.targetWorldLocation, Mathf.SmoothStep(0, 1, percentTraveled));

            yield return null;
        }

        objectToMove.currentWorldLocation = objectToMove.targetWorldLocation;
    }

    private IEnumerator DestroyHazardCoroutine(GridObject objectToDestroy, float delay = 0.0f)
    {
        Debug.Log("DestroyHazardCoroutine() called.");
        yield return new WaitForSeconds(delay);
        Destroy(objectToDestroy.gameObject);
        Debug.Log("DestroyHazardCoroutine() ended.");

        //TODO: Spawn explosion here
    }
}