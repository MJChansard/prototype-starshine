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

    private List<Hazard> hazardsInPlay = new List<Hazard>();
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
    [SerializeField] private Hazard[] hazardPrefabs;
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

            CreateHazard(spawnQueue.Dequeue());
        }
        else
        {
            AddSpawnStep();
            CreateHazard(spawnQueue.Dequeue());
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

        int hazardSelector = Random.Range(0, hazardPrefabs.Length);
        Vector2Int hazardSpawnLocation = new Vector2Int();

        // DEBUG: Make sure spawn selection is working appropriately
        if (VerboseConsole) Debug.LogFormat("Array Length: {0}, Random value: {1}", hazardPrefabs.Length, hazardSelector);

        // Identify an appropriate spawn location
        List<Vector2Int> availableSpawns = gm.GetSpawnLocations(hazardPrefabs[hazardSelector].spawnRules);
        Vector2Int targetLocation = availableSpawns[Random.Range(0, availableSpawns.Count)];
        hazardSpawnLocation.Set(targetLocation.x, targetLocation.y);

        SpawnStep newSpawnStep = ScriptableObject.CreateInstance<SpawnStep>();
        newSpawnStep.Init(hazardPrefabs[hazardSelector].hazardType, hazardSpawnLocation);
        spawnQueue.Enqueue(newSpawnStep);
    }

    private void CreateHazard(SpawnStep spawnStep)
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

        if (VerboseConsole) Debug.Log("HazardManager.CreateHazard() called.");

        // Question Pat about this part
        int hazardIndex = 0;
        for (int i = 0; i < hazardPrefabs.Length; i++)
        {
            if (hazardPrefabs[i].hazardType == spawnStep.HazardType)
            {
                hazardIndex = i;
                break;
            }
        }

        Hazard hazardToSpawn = Instantiate(hazardPrefabs[hazardIndex]);

        string borderName = "";
        if (spawnStep.SpawnLocation.y == gm.BoundaryBottomActual) borderName = "Bottom";
        else if (spawnStep.SpawnLocation.y == gm.BoundaryTopActual) borderName = "Top";
        else if (spawnStep.SpawnLocation.x == gm.BoundaryRightActual) borderName = "Right";
        else if (spawnStep.SpawnLocation.x == gm.BoundaryLeftActual) borderName = "Left";

        //Debug line
        //Hazard hazardToSpawn = Instantiate(hazardPrefabs[1]);

        hazardToSpawn.SetHazardAnimationMode(Hazard.HazardMode.Spawn, hazardToSpawn.hazardType);
        hazardToSpawn.GetComponent<Health>().ToggleInvincibility(true);

        MovePattern spawnMovement = hazardToSpawn.GetComponent<MovePattern>();
        Rotator spawnRotator = hazardToSpawn.GetComponent<Rotator>();

        switch (borderName)
        {
            case "Bottom":
                spawnMovement.SetMovePatternUp();
                spawnRotator.ApplyRotation(hazardToSpawn.hazardType, borderName);
                break;

            case "Top":
                if (hazardToSpawn.spawnRules.requiresOrientation)
                {
                    hazardToSpawn.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                    hazardToSpawn.spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                }

                spawnMovement.SetMovePatternDown();
                spawnRotator.ApplyRotation(hazardToSpawn.hazardType, borderName);
                break;

            case "Right":
                if (hazardToSpawn.spawnRules.requiresOrientation)
                {
                    hazardToSpawn.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                    hazardToSpawn.spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                }

                spawnMovement.SetMovePatternLeft();
                spawnRotator.ApplyRotation(hazardToSpawn.hazardType, borderName);
                break;

            case "Left":
                if (hazardToSpawn.spawnRules.requiresOrientation)
                {
                    hazardToSpawn.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                    hazardToSpawn.spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                }

                spawnMovement.SetMovePatternRight();
                spawnRotator.ApplyRotation(hazardToSpawn.hazardType, borderName);
                break;
        }

        AddHazard(hazardToSpawn, spawnStep.SpawnLocation);

        if (VerboseConsole) Debug.Log("HazardManager.CreateHazard() completed.");
    }

    public void AddHazard(Hazard hazard, Vector2Int gridLocation, bool placeOnGrid = true)
    {
        Debug.Log("GridObjectManager.AddHazard() called.");

        //GridBlock destinationGridPosition = gm.FindGridBlockByLocation(gridLocation);
        Vector3 worldLocation = gm.GridToWorld(gridLocation);

        //if(destinationGridPosition.IsAvailableForPlayer && placeOnGrid == false)
        if (placeOnGrid == false)
        {
            hazard.currentWorldLocation = worldLocation;
            hazard.targetWorldLocation = worldLocation;

            hazardsInPlay.Add(hazard);
        }
        //else if(!destinationGridPosition.IsAvailableForPlayer)
        else
        {
            hazard.transform.position = worldLocation;
            gm.AddObjectToGrid(hazard.gameObject, gridLocation);

            hazard.currentWorldLocation = worldLocation;
            hazard.targetWorldLocation = worldLocation;

            hazardsInPlay.Add(hazard);
        }
    }

    public void RemoveHazardFromPlay(Hazard hazard, bool removeFromGrid = true)
    {
        GameObject hazardToRemove = hazard.gameObject;
        Vector2Int gridPosition = gm.FindGridBlockContainingObject(hazardToRemove).location;

        if (removeFromGrid) gm.RemoveObjectFromGrid(hazardToRemove, gridPosition);
        hazardsInPlay.Remove(hazard);
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


    private IEnumerator HazardDropLoot(Hazard hazard, Vector3 dropLocation, float delayAppear = 1.0f)
    {
        LootHandler lh = hazard.gameObject.GetComponent<LootHandler>();
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
         *  1) Hazard Health Check
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
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            if (!CheckHazardHasHealth(hazardsInPlay[i].gameObject))
            {
                StartCoroutine(DestroyHazardCoroutine(hazardsInPlay[i]));

                // Current location for processing damage during Player phase
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].currentWorldLocation); 
                StartCoroutine(HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].currentWorldLocation, 0.0f));

                RemoveHazardFromPlay(hazardsInPlay[i]);
            }
        }

        // Movement and Collision collections
        List<GridBlock> allPossibleBlockCollisions = new List<GridBlock>();
        Vector2Int[] allOriginGridLocations = new Vector2Int[hazardsInPlay.Count];
        Vector2Int[] allDestinationGridLocations = new Vector2Int[hazardsInPlay.Count];

        // Manage Movement Data
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = hazardsInPlay[i].gameObject;
            MovePattern move = hazardObject.GetComponent<MovePattern>();
            move.OnTickUpdate();

            Vector2Int originGridLocation = gm.WorldToGrid(hazardsInPlay[i].currentWorldLocation);
            allOriginGridLocations[i] = originGridLocation;

            if (move.CanMoveThisTurn())
            {
                Debug.Log(hazardsInPlay[i].HazardName + " is moving by " + move.delta);

                Vector2Int destinationGridLocation = originGridLocation + move.delta;
                allDestinationGridLocations[i] = destinationGridLocation;

                bool moveInBounds = gm.CheckIfGridBlockInBounds(destinationGridLocation);

                if (!moveInBounds)
                {
                    StartCoroutine(DestroyHazardCoroutine(hazardsInPlay[i], 2.0f));
                    RemoveHazardFromPlay(hazardsInPlay[i]);
                    hazardDestroyedThisTick = true;
                }
                else
                {
                    gm.RemoveObjectFromGrid(hazardObject, originGridLocation);
                    Debug.Log("Removing " + hazardsInPlay[i].HazardName + " from " + originGridLocation.ToString());

                    gm.AddObjectToGrid(hazardObject, destinationGridLocation);
                    Debug.Log("Adding " + hazardsInPlay[i].HazardName + " to " + destinationGridLocation.ToString());

                    // Handle spawning cases
                    hazardObject.GetComponent<Health>().ToggleInvincibility(false);
                    hazardsInPlay[i].SetHazardAnimationMode(Hazard.HazardMode.Play, hazardsInPlay[i].hazardType);

                    Rotator hazardRotator = hazardsInPlay[i].GetComponent<Rotator>();
                    if (hazardRotator != null) hazardRotator.enabled = true;

                    hazardsInPlay[i].targetWorldLocation = gm.GridToWorld(destinationGridLocation);

                    allPossibleBlockCollisions.Add(gm.FindGridBlockByLocation(destinationGridLocation));
                    //InsertGridBlockCollision(destinationGridLocation);    -- Try this!
                }
            }
            else
            {
                hazardsInPlay[i].targetWorldLocation = hazardsInPlay[i].currentWorldLocation;
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

                    Hazard flyByHazard1 = hazardsInPlay[i];
                    Hazard flyByHazard2 = hazardsInPlay[j];

                    Health flyByHazard1HP = flyByHazard1.gameObject.GetComponent<Health>();
                    flyByHazard1HP.SubtractHealth(flyByHazard2.GetComponent<ContactDamage>().DamageAmount);

                    Health flyByHazard2HP = flyByHazard2.gameObject.GetComponent<Health>();
                    flyByHazard2HP.SubtractHealth(flyByHazard1.GetComponent<ContactDamage>().DamageAmount);

                    if (flyByHazard1.HazardName == "Missile" || flyByHazard2.HazardName == "Missile")
                    {
                        if (flyByHazard1.HazardName == "Missile") StartCoroutine(MoveHazardCoroutine(flyByHazard2, 1.0f));
                        if (flyByHazard2.HazardName == "Missile") StartCoroutine(MoveHazardCoroutine(flyByHazard1, 1.0f));
                    }
                    else if (!CheckHazardHasHealth(flyByHazard1.gameObject) && !CheckHazardHasHealth(flyByHazard2.gameObject))
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
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = hazardsInPlay[i].gameObject;

            if (!CheckHazardHasHealth(hazardObject))
            {
                StartCoroutine(DestroyHazardCoroutine(hazardsInPlay[i], 2.0f));
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation); // Target location for Fly-By cases
                StartCoroutine(HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation));
                RemoveHazardFromPlay(hazardsInPlay[i]);
                hazardDestroyedThisTick = true;
            }
        }

        // Move hazards
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            if (hazardsInPlay[i].currentWorldLocation != hazardsInPlay[i].targetWorldLocation)
            {
                StartCoroutine(MoveHazardCoroutine(hazardsInPlay[i]));
                moveOccurredThisTick = true;
            }
        }

        // GridBlock Collisions
        uniquePossibleBlockCollisions = allPossibleBlockCollisions.Distinct().ToList();
        ProcessCollisionsOnGridBlock(ref uniquePossibleBlockCollisions);

        // Hazard Health Check following Hazard movement
        for (int i = hazardsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = hazardsInPlay[i].gameObject;

            if (!CheckHazardHasHealth(hazardObject))
            {
                StartCoroutine(DestroyHazardCoroutine(hazardsInPlay[i], 1.0f));
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation); // Drop loot where hazard will end up after MoveHazardCoroutine();
                StartCoroutine(HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation));
                RemoveHazardFromPlay(hazardsInPlay[i]);
                hazardDestroyedThisTick = true;
            }
        }

        // Spawn stuff
        if (ticksUntilNewSpawn == 0 || hazardsInPlay.Count == 0)
        {
            if (spawnQueue.Count > 0)
            {
                CreateHazard(spawnQueue.Dequeue());
            }
            else
            {
                AddSpawnStep();
                CreateHazard(spawnQueue.Dequeue());
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
                    Hazard hazard = gameObject.GetComponent<Hazard>();
                    Health gameObjectHealth = gameObject.GetComponent<Health>();
                    ContactDamage gameObjectDamage = gameObject.GetComponent<ContactDamage>();

                    for (int j = 1 + i; j < gridBlock.objectsOnBlock.Count; j++)
                    {
                        GameObject otherGameObject = gridBlock.objectsOnBlock[j];
                        Hazard otherHazard = otherGameObject.GetComponent<Hazard>();
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

                        if (gameObject.CompareTag("Player") && otherHazard != null)
                        {
                            if (otherHazard.hazardType == Hazard.HazardType.AmmoCrate)
                            {
                                LootData ammo = otherHazard.GetComponent<LootData>();
                                pm.AcceptLoot(ammo.Type, ammo.LootAmount);
                                otherGameObjectHealth.SubtractHealth(otherGameObjectHealth.CurrentHP);
                            }
                        }

                        if (hazard != null && otherGameObject.CompareTag("Player"))
                        {
                            if (hazard.hazardType == Hazard.HazardType.AmmoCrate)
                            {
                                LootData ammo = hazard.GetComponent<LootData>();
                                pm.AcceptLoot(ammo.Type, ammo.LootAmount);
                                gameObjectHealth.SubtractHealth(gameObjectHealth.CurrentHP);
                            }
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



    private IEnumerator MoveHazardCoroutine(Hazard hazardToMove, float hazardTravelLength = 1.0f)
    {
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= hazardTravelLength)
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