﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridObjectManager : MonoBehaviour
{
    #region Grid Object Manager Data   
    [SerializeField] private bool VerboseConsole = true;

    private GridManager gm;
    private Player pm;

    private int currentTick = 0;

    private Vector2Int minVector2;
    private Vector2Int maxVector2;

    private List<GridObject> gridObjectsInPlay = new List<GridObject>();            // Object tracking
    private List<TickBehavior> behaviorProcessing = new List<TickBehavior>();       // Object behavior on each tick
    private List<GridBlock> potentialBlockCollisions = new List<GridBlock>();       // Collision tracker

    private class TickBehavior
    {
        public GridObject gridObject;
        public TickOutcome tickOutcome;

        public enum TickOutcome
        {
            Undecided = 0,
            Nothing = 1,
            Depart = 2,
            Move = 3,
            FlyBy = 4,
            
        }

        public TickBehavior(GridObject _gridObject, TickOutcome _outcome = TickOutcome.Undecided)
        {
            this.gridObject = _gridObject;
            this.tickOutcome = _outcome;
        }
    }

    //List<GridBlock> allPossibleBlockCollisions = new List<GridBlock>();
    #endregion

    public enum GamePhase
    {
        Player = 1,
        Manager = 2
    }
    private GamePhase currentPhase;
    


    #region Object Spawning
    [Header("Spawn Management")]
    [SerializeField] private int minTicksUntilSpawn = 2;
    [SerializeField] private int maxTicksUntilSpawn = 4;

    [HideInInspector] public List<SpawnSequence> insertSpawnSequences = new List<SpawnSequence>();
    private Queue<SpawnStep> spawnQueue = new Queue<SpawnStep>();
    private int ticksUntilNewSpawn;

    [Header("Grid Object Inventory")]
    [SerializeField] private GridObject[] gridObjectPrefabs;
    public GameObject playerPrefab;
    #endregion


    private void Start()
    {
        gm = GetComponent<GridManager>();

        minVector2 = new Vector2Int(gm.BoundaryLeftActual, gm.BoundaryBottomActual);
        maxVector2 = new Vector2Int(gm.BoundaryRightActual, gm.BoundaryTopActual);

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);

        if (VerboseConsole)
        {
            Debug.LogFormat("Length of uniquePossibleBlockCollisions: {0}", potentialBlockCollisions.Count);
        }
    }

    public void Init()
    {
        currentTick = 1;
        currentPhase = GamePhase.Player;

        gridObjectsInPlay[0] = GameObject.FindWithTag("Player").GetComponent<Player>();

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
        
        AddObjectToGrid(newSpawn, spawnStep.SpawnLocation);

        if (VerboseConsole) Debug.Log("GridObjectManager.CreateGridObject() completed.");
    }

    public void AddObjectToGrid(GridObject gridObject, Vector2Int gridLocation, bool placeOnGrid = true)
    {
        Debug.Log("GridObjectManager.AddHazard() called.");

        Vector3 worldLocation = gm.GridToWorld(gridLocation);

        if (placeOnGrid == false)
        {
            gridObject.currentWorldLocation = worldLocation;
            gridObject.targetWorldLocation = worldLocation;

            gridObjectsInPlay.Add(gridObject);
        }
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

   

    //private void UpdateGridObjectMovement(List<GridObject> objects)
    // This signature means I can use this method to process either Player phase objects or Manager phase objects
    private List<TickBehavior> SetGridObjectBehaviorForTick(List<GridObject> objects)
    {
        /*  PLAN
         *   - Process a list of GridObjects' data
         *   - Determine one of the following behaviors for the current tick for each object:
         *      ~ Depart the grid
         *      ~ Simply move
         *      ~ FlyBy another object
         *      ~ Nothing
         */

        List<TickBehavior> returnList = new List<TickBehavior>();

        Vector2Int[] allOriginGridLocations = new Vector2Int[objects.Count];
        Vector2Int[] allDestinationGridLocations = new Vector2Int[objects.Count];

        for (int i = 0; i < objects.Count; i++)
        {
            allOriginGridLocations[i].Set(99, 99);
            allDestinationGridLocations[i].Set(99, 99);
        }

        for (int i = 0; i < objects.Count; i++)
        {
            TickBehavior behavior = new TickBehavior(objects[i]);

            MovePattern targetMovement = objects[i].GetComponent<MovePattern>();
            if (targetMovement.CanMoveThisTurn)
            {
                Vector2Int currentLocation = gm.WorldToGrid(objects[i].currentWorldLocation);
                Vector2Int destinationLocation = currentLocation + targetMovement.delta;

                if (gm.CheckIfGridBlockInBounds(destinationLocation))
                {
                    allOriginGridLocations[i] = currentLocation;            // Maintain index of objects requiring additional processing
                    allDestinationGridLocations[i]  = destinationLocation;

                    returnList.Add(behavior);       // This is the TickBehavior that will be updated in Fly-By
                }
                else
                {
                    behavior.tickOutcome = TickBehavior.TickOutcome.Depart;
                    returnList.Add(behavior);
                }
            }
            else 
            {
                behavior.tickOutcome = TickBehavior.TickOutcome.Nothing;
                returnList.Add(behavior);
            }
        }
        
        // Process remaining TickBehavior.TickOutcome.Undecided behaviors
        // Fly-By detection
        if (VerboseConsole) Debug.Log("Fly-By detection starting.");

        for (int i = 0; i < allOriginGridLocations.Length; i++)
        {
            for (int j = 0; j < allDestinationGridLocations.Length; j++)
            {
                if (i == j) continue;
                else if (allOriginGridLocations[i].x == 99) continue;
                else if (allDestinationGridLocations[j].x == 99) continue;
                else if (allOriginGridLocations[i] == allDestinationGridLocations[j] && allOriginGridLocations[j] == allDestinationGridLocations[i])
                {
                    returnList[i].tickOutcome = TickBehavior.TickOutcome.FlyBy;
                    returnList[j].tickOutcome = TickBehavior.TickOutcome.FlyBy;
                }
                else returnList[i].tickOutcome = TickBehavior.TickOutcome.Move;
            }
        }

        return returnList;
    }

    //public void MoveGridObject(GridObject target)
    private void ExecuteGridObjectBehaviorForTick(List<TickBehavior> objects)
    {
        /*  SUMMARY
         * 
         *  Process TickBehavior for every GridObject
         *   - TickOutcome.Depart
         *      ~ Remove GridObject from play
         *
         *   - TickOutcome.Move
         *      ~ Remove GridObject from current location
         *      ~ Place GridObject at destination location
         *      ~ Add destination location to Collision Test List
         *      ~ Start movement coroutines
         */

        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].tickOutcome == TickBehavior.TickOutcome.Nothing) continue;
            else if (objects[i].tickOutcome == TickBehavior.TickOutcome.Depart)
            {
                StartCoroutine(GridObjectDestructionCoroutine(objects[i].gridObject, 2.0f));
                RemoveGridObjectFromPlay(objects[i].gridObject);
            }
            else if (objects[i].tickOutcome == TickBehavior.TickOutcome.Move)
            {
                MovePattern moveTarget = objects[i].gridObject.GetComponent<MovePattern>();
                moveTarget.OnTickUpdate();

                Vector2Int currentLocation = gm.WorldToGrid(objects[i].gridObject.currentWorldLocation);
                Vector2Int destinationLocation = currentLocation + moveTarget.delta;

                // I don't like how many calls to GridManager this is making.  #Optimize
                gm.RemoveObjectFromGrid(objects[i].gridObject.gameObject, currentLocation);
                gm.AddObjectToGrid(objects[i].gridObject.gameObject, destinationLocation);

                objects[i].gridObject.targetWorldLocation = gm.GridToWorld(destinationLocation);

                InsertGridBlockCollision(destinationLocation);
                StartCoroutine(GridObjectMovementCoroutine(objects[i].gridObject, 1.0f));
            }
            else if (objects[i].tickOutcome == TickBehavior.TickOutcome.FlyBy)
            {
                for (int j = 0; j < objects.Count; j++)
                {
                    if (i == j) continue;

                    if (objects[j].tickOutcome != TickBehavior.TickOutcome.FlyBy) continue;

                    if (objects[i].gridObject.targetWorldLocation == objects[j].gridObject.currentWorldLocation &&
                        objects[i].gridObject.currentWorldLocation == objects[j].gridObject.targetWorldLocation)
                    {
                        Health hp = objects[i].gridObject.GetComponent<Health>();
                        hp.SubtractHealth(objects[j].gridObject.GetComponent<ContactDamage>().DamageAmount);

                        if(hp.HasHP) StartCoroutine(GridObjectMovementCoroutine(objects[i].gridObject, 1.0f));

                        // Possibility of executing GridObjectDestructionCoroutine here
                        // Right now, I want to do a GridObject health check after this method completes
                    }
                }
            }
            else Debug.LogFormat("No TickBehavior.TickOutcome set for {0}.", objects[i].gridObject.ToString());
        }
    }

    private bool ProcessGridObjectHealth(List<GridObject> objects)
    {
        for (int i = objects.Count - 1; i > -1; i--)
        {
            Health hp = objects[i].GetComponent<Health>();
            if (hp != null && hp.HasHP)
            {
                StartCoroutine(GridObjectDestructionCoroutine(objects[i]));
                StartCoroutine(DropLootCoroutine(objects[i], objects[i].currentWorldLocation, 0.0f));
                RemoveGridObjectFromPlay(objects[i]);

                return true;
            }
        }

        return false;
    }

    private bool CheckGridObjectHasHealth(GridObject checkObject)
    {
        // This might better be suited as a Property in Health.cs

        Health hp = checkObject.GetComponent<Health>();
        if (hp != null && hp.HasHP)
        {
            return true;
        }
        return false;
    }


    private IEnumerator DropLootCoroutine(GridObject gridObject, Vector3 dropLocation, float delayAppear = 1.0f)
    {
        LootHandler lh = gridObject.gameObject.GetComponent<LootHandler>();
        if (lh != null)
        {
            GameObject lootObjectToDrop = lh.RequestLootDrop(dropLocation, forced: true);

            if (lootObjectToDrop != null)
            {
                MeshRenderer renderer = lootObjectToDrop.GetComponent<MeshRenderer>();
                renderer.enabled = false;

                gm.AddObjectToGrid(lootObjectToDrop, gm.WorldToGrid(dropLocation));
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
        bool moveOccurredThisTick = true;
        float moveDurationSeconds = 1.0f;

        bool gridObjectDestroyedThisTick = false;
        float destroyDurationSeconds = 2.0f;

        float delayTime = 0.0f;
        #endregion

        // Player collision check
        if (potentialBlockCollisions.Count > 0) ProcessCollisionsOnGridBlock(ref potentialBlockCollisions);

        // Hazard Health Check - Check for hazards destroyed during Player turn
        ProcessGridObjectHealth(gridObjectsInPlay);
        /*
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            if (!CheckGridObjectHasHealth(gridObjectsInPlay[i]))
            {
                StartCoroutine(GridObjectDestructionCoroutine(gridObjectsInPlay[i]));
                StartCoroutine(DropLoot(gridObjectsInPlay[i], gridObjectsInPlay[i].currentWorldLocation, 0.0f));
                RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
            }
        }
        */
        // Set GridObject behavior for current Tick
        List<TickBehavior> currentTick = SetGridObjectBehaviorForTick(gridObjectsInPlay);

        // Process GridObject behavior for current Tick
        ExecuteGridObjectBehaviorForTick(currentTick);
        gridObjectDestroyedThisTick = ProcessGridObjectHealth(gridObjectsInPlay);

        /*
        // Manage Movement Data
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = gridObjectsInPlay[i].gameObject;
            MovePattern move = hazardObject.GetComponent<MovePattern>();
            move.OnTickUpdate();

            Vector2Int originGridLocation = gm.WorldToGrid(gridObjectsInPlay[i].currentWorldLocation);
            allOriginGridLocations[i] = originGridLocation;

            if (move.CanMoveThisTurn)
            {
                Debug.Log(gridObjectsInPlay[i].ToString() + " is moving by " + move.delta);

                Vector2Int destinationGridLocation = originGridLocation + move.delta;
                allDestinationGridLocations[i] = destinationGridLocation;

                bool moveInBounds = gm.CheckIfGridBlockInBounds(destinationGridLocation);

                if (!moveInBounds)
                {
                    StartCoroutine(GridObjectDestructionCoroutine(gridObjectsInPlay[i], 2.0f));
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
                    else
                    if (!CheckGridObjectHasHealth(flyByHazard1.gameObject) && !CheckGridObjectHasHealth(flyByHazard2.gameObject))
                    {
                        StartCoroutine(GridObjectMovementCoroutine(flyByHazard1, 1.0f));
                    }
                    else if (!CheckGridObjectHasHealth(flyByHazard1.gameObject))
                    {
                        // If flyByHazard1 did not survive ...
                        StartCoroutine(GridObjectMovementCoroutine(flyByHazard1, 1.0f));
                    }
                    else if (!CheckGridObjectHasHealth(flyByHazard2.gameObject))
                    {
                        // If flyByHazard2 did not survive...
                        StartCoroutine(GridObjectMovementCoroutine(flyByHazard2, 1.0f));
                    }
                }
            }
        }
        

        // Hazard Health check following Fly-By processing
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = gridObjectsInPlay[i].gameObject;

            if (!CheckGridObjectHasHealth(hazardObject))
            {
                StartCoroutine(GridObjectDestructionCoroutine(gridObjectsInPlay[i], 2.0f));
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation); // Target location for Fly-By cases
                StartCoroutine(DropLoot(gridObjectsInPlay[i], gridObjectsInPlay[i].targetWorldLocation));
                RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
                gridObjectDestroyedThisTick = true;
            }
        }
        
        // Move hazards
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            if (gridObjectsInPlay[i].currentWorldLocation != gridObjectsInPlay[i].targetWorldLocation)
            {
                StartCoroutine(GridObjectMovementCoroutine(gridObjectsInPlay[i]));
                moveOccurredThisTick = true;
            }
        }
        */
        // GridBlock Collisions
        //potentialBlockCollisions = allPossibleBlockCollisions.Distinct().ToList();
        ProcessCollisionsOnGridBlock(ref potentialBlockCollisions);

        // Hazard Health Check following Hazard movement
        /*
        for (int i = gridObjectsInPlay.Count - 1; i > -1; i--)
        {
            GameObject hazardObject = gridObjectsInPlay[i].gameObject;

            if (!CheckGridObjectHasHealth(hazardObject))
            {
                StartCoroutine(GridObjectDestructionCoroutine(gridObjectsInPlay[i], 1.0f));
                //HazardDropLoot(hazardsInPlay[i], hazardsInPlay[i].targetWorldLocation); // Drop loot where hazard will end up after MoveHazardCoroutine();
                StartCoroutine(DropLootCoroutine(gridObjectsInPlay[i], gridObjectsInPlay[i].targetWorldLocation));
                RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
                gridObjectDestroyedThisTick = true;
            }
        }
        */
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
        if (gridObjectDestroyedThisTick) delayTime += destroyDurationSeconds;
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
        for (int i = 0; i < potentialBlockCollisions.Count; i++)
        {
            if (potentialBlockCollisions[i].location != gridLocation)
            {
                continue;
            }
            else
            {
                Debug.Log("Location already in Collision List.");
                break;
            }
        }

        potentialBlockCollisions.Add(gm.FindGridBlockByLocation(gridLocation));
    }



    private IEnumerator GridObjectMovementCoroutine(GridObject objectToMove, float travelLength = 1.0f)
    {
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= travelLength)
        {
            float traveled = (Time.time - startTime) * 1.0f;
            percentTraveled = traveled / objectToMove.Distance;
            objectToMove.transform.position = Vector3.Lerp(objectToMove.currentWorldLocation, objectToMove.targetWorldLocation, Mathf.SmoothStep(0, 1, percentTraveled));

            yield return null;
        }

        objectToMove.currentWorldLocation = objectToMove.targetWorldLocation;
        StartCoroutine(DropLootCoroutine(objectToMove, objectToMove.currentWorldLocation));
    }

    private IEnumerator GridObjectDestructionCoroutine(GridObject objectToDestroy, float delay = 0.0f)
    {
        Debug.Log("DestroyHazardCoroutine() called.");
        yield return new WaitForSeconds(delay);
        Destroy(objectToDestroy.gameObject);
        Debug.Log("DestroyHazardCoroutine() ended.");

        //TODO: Spawn explosion here
    }
}