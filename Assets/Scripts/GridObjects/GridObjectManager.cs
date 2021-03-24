using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridObjectManager : MonoBehaviour {
    #region Grid Object Manager Data   
    [SerializeField] private bool VerboseConsole = true;

    private GridManager gm;
    private Player player;

    private int currentTick = 0;

    private Vector2Int minVector2;
    private Vector2Int maxVector2;

    private List<GridObject> gridObjectsInPlay = new List<GridObject>();            // Object tracking
    private List<TickBehavior> behaviorProcessing = new List<TickBehavior>();       // Object behavior on each tick
    //private List<GridBlock> potentialBlockCollisions = new List<GridBlock>();       // Collision tracker

    private class TickBehavior {
        public GridObject gridObject;
        public TickOutcome tickOutcome;

        public enum TickOutcome {
            Undecided = 0,
            Nothing = 1,
            Depart = 2,
            Move = 3,
            FlyBy = 4,
        }

        public TickBehavior(GridObject _gridObject, TickOutcome _outcome = TickOutcome.Undecided) {
            this.gridObject = _gridObject;
            this.tickOutcome = _outcome;
        }
    }
    #endregion

    public enum GamePhase {
        Player = 1,
        Manager = 2
    }
    //private GamePhase currentPhase;



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


    private void Start() {
        gm = GetComponent<GridManager>();

        minVector2 = new Vector2Int(gm.BoundaryLeftActual, gm.BoundaryBottomActual);
        maxVector2 = new Vector2Int(gm.BoundaryRightActual, gm.BoundaryTopActual);

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
    }

    public void Init() {
        currentTick = 1;

        gridObjectsInPlay.Insert(0, GameObject.FindWithTag("Player").GetComponent<Player>());
        player = gridObjectsInPlay[0].GetComponent<Player>();

        if (insertSpawnSequences.Count > 0) {
            for (int i = 0; i < insertSpawnSequences.Count; i++) {
                for (int j = 0; j < insertSpawnSequences[i].hazardSpawnSteps.Length; j++) {
                    spawnQueue.Enqueue(insertSpawnSequences[i].hazardSpawnSteps[j]);
                }
            }

            CreateGridObject(spawnQueue.Dequeue());
        } else {
            AddSpawnStep();
            CreateGridObject(spawnQueue.Dequeue());
        }
    }

    // Move to Grid Manager
    // Move generic spawn rule processing stuff into Grid Manager
    // Hey GM, give me a list of all generic spawn locations
    // Then HM further wittles down the list based on hazard specific criteria
    // HM.GetLocationsInHazardPaths()


    private void AddSpawnStep() {
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
    private void CreateGridObject(SpawnStep spawnStep) {
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
    public void AddObjectToGrid(GridObject gridObject, Vector2Int gridLocation, bool placeOnGrid = true) {
        Debug.Log("GridObjectManager.AddHazard() called.");

        Vector3 worldLocation = gm.GridToWorld(gridLocation);

        if (placeOnGrid == false) {
            gridObject.currentWorldLocation = worldLocation;
            gridObject.targetWorldLocation = worldLocation;

            gridObjectsInPlay.Add(gridObject);
        } else {
            gridObject.transform.position = worldLocation;
            gm.AddObjectToGrid(gridObject.gameObject, gridLocation);

            gridObject.currentWorldLocation = worldLocation;
            gridObject.targetWorldLocation = worldLocation;

            gridObjectsInPlay.Add(gridObject);
        }
    }
    public void RemoveGridObjectFromPlay(GridObject objectToRemove, bool removeFromGrid = true) {
        GameObject gameObjectToRemove = objectToRemove.gameObject;
        Vector2Int gridPosition = gm.FindGridBlockContainingObject(gameObjectToRemove).location;

        if (removeFromGrid) gm.RemoveObjectFromGrid(gameObjectToRemove, gridPosition);
        gridObjectsInPlay.Remove(objectToRemove);
    }



    private List<GridBlock> MoveGridObjectsForTick(List<GridObject> objects) {
        /*  PLAN
         *   - Process a list of GridObjects' data
         *   - Determine one of the following behaviors for the current tick for each object:
         *      ~ Depart the grid
         *      ~ Simply move
         *      ~ FlyBy another object
         *      ~ Nothing
         */

        //List<TickBehavior> returnList = new List<TickBehavior>();

        Vector2Int[] allOriginGridLocations = new Vector2Int[objects.Count];
        Vector2Int[] allDestinationGridLocations = new Vector2Int[objects.Count];

        List<GridBlock> collisionTestBlocks = new List<GridBlock>();

        //List<Vector2Int> allOriginGridLocations = new List<Vector2Int>();
        //List<Vector2Int> allDestinationGridLocations = new List<Vector2Int>();

        for (int i = 0; i < objects.Count; i++)
        {
            allOriginGridLocations[i].Set(99, 99);
            allDestinationGridLocations[i].Set(99, 99);
        }

        // Process movement data
        for (int i = 0; i < objects.Count; i++)
        {
            MovePattern mp = objects[i].GetComponent<MovePattern>();
            mp.OnTickUpdate();
            if (mp.CanMoveThisTurn)
            {
                Vector2Int currentLocation = gm.WorldToGrid(objects[i].currentWorldLocation);
                Vector2Int destinationLocation = currentLocation + mp.DirectionOnGrid;

                if (gm.CheckIfGridBlockInBounds(destinationLocation))
                {
                    // Move or FlyBy
                    allOriginGridLocations[i] = currentLocation;            // Maintain index of objects requiring additional processing
                    allDestinationGridLocations[i] = destinationLocation;

                    //allOriginGridLocations.Add(currentLocation);
                }
                else
                {
                    // Depart
                    objects[i].IsLeavingGrid = true;
                }
            }
        }

        if (VerboseConsole) Debug.Log("Fly-By detection starting.");

        for (int i = 0; i < allOriginGridLocations.Length; i++)
        {
            for (int j = 0; j < allDestinationGridLocations.Length; j++)
            {
                if (objects.Count > 1 && i == j) continue;
                else if (allOriginGridLocations[i].x == 99) continue;
                else if (allDestinationGridLocations[j].x == 99) continue;
                else if (allOriginGridLocations[i] == allDestinationGridLocations[j] && allOriginGridLocations[j] == allDestinationGridLocations[i]) {
                    Health hp = objects[i].GetComponent<Health>();
                    hp.SubtractHealth(objects[j].GetComponent<ContactDamage>().DamageAmount);

                    //if (hp.HasHP) StartCoroutine(GridObjectMovementCoroutine(objects[i], 1.0f));

                    // Possibility of executing GridObjectDestructionCoroutine here
                    // Right now, I want to do a GridObject health check after this method completes
                }
            }
        }

        if (VerboseConsole) Debug.Log("Moving GridObjects.");

        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].CurrentMode == GridObject.Mode.Spawn)
                objects[i].SetGamePlayMode(GridObject.Mode.Play);

            MovePattern mp = objects[i].GetComponent<MovePattern>();
            //mp.OnTickUpdate();

            Vector2Int currentLocation = gm.WorldToGrid(objects[i].currentWorldLocation);
            Vector2Int destinationLocation = currentLocation + mp.DirectionOnGrid;

            // I don't like how many calls to GridManager this is making.  #Optimize
            if (mp.CanMoveThisTurn)
            {
                if (objects[i].IsLeavingGrid == false && objects[i].GetComponent<Health>().HasHP)
                {
                    gm.RemoveObjectFromGrid(objects[i].gameObject, currentLocation);
                    gm.AddObjectToGrid(objects[i].gameObject, destinationLocation);

                    objects[i].targetWorldLocation = gm.GridToWorld(destinationLocation);

                    //InsertGridBlockCollision(destinationLocation);
                    if(!IsLocationInCollisionTracker(destinationLocation, collisionTestBlocks))
                    {
                        collisionTestBlocks.Add(gm.FindGridBlockByLocation(destinationLocation));
                    }

                    StartCoroutine(GridObjectMovementCoroutine(objects[i], 1.0f));
                }
                else
                {   // Departing
                    objects[i].targetWorldLocation = gm.GridToWorld(destinationLocation);

                    StartCoroutine(GridObjectMovementCoroutine(objects[i], 1.0f));
                    StartCoroutine(GridObjectDestructionCoroutine(objects[i], 1.1f));
                }
            }
        }

        return collisionTestBlocks;
    }

    public float OnTickUpdate(GamePhase phase)
    {
        /*  STEPS
         * 
         *  1) GridObject Health Check
         *  2) Move hazards
         *  3) Detect Fly-Bys
         *  4) Hazard Health Check
         *  5) Collisions on a GridBlock
         */

        #region Tick Duration
        bool moveOccurredThisTick = true;
        float moveDurationSeconds = 1.0f;

        bool gridObjectDestroyedThisTick = false;
        float destroyDurationSeconds = 2.0f;

        float delayTime = 0.0f;
        #endregion

        List<GridObject> objectProcessing = new List<GridObject>();
        List<GridBlock> potentialBlockCollisions = new List<GridBlock>();

        if (phase == GamePhase.Player)
        {
            if (!player.IsAttackingThisTick)
            {
                for (int i = 0; i < gridObjectsInPlay.Count; i++)
                {
                    if (gridObjectsInPlay[i].ProcessingPhase == GamePhase.Player)
                        objectProcessing.Add(gridObjectsInPlay[i]);
                }
                //potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);
            }
            else
            {
                for (int i = 0; i < gridObjectsInPlay.Count; i++)
                {
                    if (gridObjectsInPlay[i].ProcessingPhase == GamePhase.Player && !gridObjectsInPlay[i].CompareTag("Player"))
                        objectProcessing.Add(gridObjectsInPlay[i]);
                }

                Weapon playerWeapon = player.SelectedWeapon;

                if (playerWeapon.weaponAmmunition > 0)
                {
                    List<GridBlock> targetBlocks = GetGridBlocksInPath(gm.WorldToGrid(player.currentWorldLocation), player.Direction);

                    if (player.SelectedWeaponRequiresInstance)
                    {
                        GameObject weaponInstance = Instantiate(player.SelectedWeaponProjectile, player.currentWorldLocation, player.transform.rotation);
                        weaponInstance.GetComponent<MovePattern>().SetMovePattern(player.Direction);

                        GridObject weaponObject = weaponInstance.GetComponent<GridObject>();
                        AddObjectToGrid(weaponObject, gm.WorldToGrid(player.currentWorldLocation));
                        weaponObject.targetWorldLocation = weaponObject.currentWorldLocation + gm.GridToWorld(player.Direction);

                        // #Optimize.  Can I feed this into objectProcessing instead of creating a new List?
                        //List<GridObject> weaponList = new List<GridObject>();
                        //weaponList.Add(weaponObject);
                        //MoveGridObjectsForTick(weaponList);

                        objectProcessing.Add(weaponObject);
                    }
                    else
                    {
                        for (int i = 0; i < targetBlocks.Count; i++)
                        {
                            for (int j = 0; j < targetBlocks[i].objectsOnBlock.Count; j++)
                            {
                                Health hp = targetBlocks[i].objectsOnBlock[j].GetComponent<Health>();
                                if (hp != null)
                                {
                                    hp.SubtractHealth(player.SelectedWeaponDamage);

                                    if (!player.SelectedWeaponDoesPenetrate)
                                    {
                                        player.ExecuteAttackAnimation(targetBlocks[i]);
                                        break;
                                    }
                                    else if (player.SelectedWeaponDoesPenetrate && i == targetBlocks.Count - 1)
                                        player.ExecuteAttackAnimation(targetBlocks[i]);
                                }
                            }
                        }
                    }
                }
            }

            potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);
            //ProcessCollisionsOnGridBlock(potentialBlockCollisions);
            player.IsAttackingThisTick = false;
        }

        if (phase == GamePhase.Manager)
        {
            ProcessGridObjectHealth(gridObjectsInPlay);

            // Process GridObject behavior for current Tick
            for (int i = 0; i < gridObjectsInPlay.Count; i++)
            {
                if (gridObjectsInPlay[i].ProcessingPhase == GamePhase.Manager)
                    if (gridObjectsInPlay[i].GetComponent<Health>().HasHP)
                        objectProcessing.Add(gridObjectsInPlay[i]);
            }
            potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);

            gridObjectDestroyedThisTick = ProcessGridObjectHealth(gridObjectsInPlay);

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
        }

        ProcessCollisionsOnGridBlock(potentialBlockCollisions);
        ProcessGridObjectHealth(gridObjectsInPlay, 1.0f);

        if (moveOccurredThisTick) delayTime += moveDurationSeconds;
        if (gridObjectDestroyedThisTick) delayTime += destroyDurationSeconds;
        return delayTime;

        //StartCoroutine(AnimateTick);
    }
    /*
List<AnimateTickBehaviors> tickBehaviors;

IEnumerator AnimateTick() {

    // ...


    // Start of hazards moving
    // do anything that needs to happen at this timestamp

    return waitForSeconds(hazardTimeDelay * 0.5);

    // do anything that needs to happen at halfway point (flybys)
    DoAnimateTickBehavior(halfway);


    return waitForSeconds(hazardTimeDelay * 0.5); 


    DoAnimateTickBehavior(end);

}

void DoAnimateTickBehavior(Phase phase) {

}
*/



/*
    public void InsertGridBlockCollision(Vector2Int gridLocation, List<GridBlock> gridList) {
        for (int i = 0; i < gridList.Count; i++)
        {
            if (gridList[i].location != gridLocation)
            {
                continue;
            }
            else
            {
                Debug.Log("Location already in Collision List.");
                break;
            }

        }
        gridList.Add(gm.FindGridBlockByLocation(gridLocation));
    }
*/
    private bool IsLocationInCollisionTracker(Vector2Int gridLocation, List<GridBlock> gridList)
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            if (gridList[i].location != gridLocation)
            {
                continue;
            }
            else
            {
                Debug.Log("Location already in Collision List.");
                return true;
            }
        }
        return false;
    }


    private void ProcessCollisionsOnGridBlock(List<GridBlock> gridBlocks)
    {    
        for (int i = 0; i < gridBlocks.Count; i++) {
            if (gridBlocks[i].objectsOnBlock.Count > 1) {
                Debug.Log("Processing collision on " + gridBlocks[i].location.ToString());

                for (int j = 0; j < gridBlocks[i].objectsOnBlock.Count; j++)
                {
                    GameObject gameObject = gridBlocks[i].objectsOnBlock[j];
                    GridObject gridObject = gameObject.GetComponent<GridObject>();
                    Loot loot = gameObject.GetComponent<Loot>();
                    Health gameObjectHealth = gameObject.GetComponent<Health>();
                    ContactDamage gameObjectDamage = gameObject.GetComponent<ContactDamage>();

                    for (int k = 1 + j; k < gridBlocks[i].objectsOnBlock.Count; k++)
                    {
                        GameObject otherGameObject = gridBlocks[i].objectsOnBlock[k];
                        GridObject otherGridObject = otherGameObject.GetComponent<GridObject>();
                        Loot otherLoot = otherGameObject.GetComponent<Loot>();
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

                        //if (gameObject.CompareTag("Player") && otherLoot != null)
                        //Player p = gridObject as Player;
                        if (gridObject is Player && otherLoot != null)
                        {
                            Player p = gridObject as Player;
                            p.AcceptLoot(otherLoot.Type, otherLoot.LootAmount);
                            //otherGameObjectHealth.SubtractHealth(otherGameObjectHealth.CurrentHP);
                            //StartCoroutine(GridObjectDestructionCoroutine())
                        }

                        //if (loot != null && otherGameObject.CompareTag("Player"))
                        if (otherGridObject is Player && loot != null)
                        {
                            Player p2 = otherGridObject as Player;
                            p2.AcceptLoot(loot.Type, loot.LootAmount);
                            gameObjectHealth.SubtractHealth(gameObjectHealth.CurrentHP);
                        }
                    }
                }
            }
        }

        //gridBlocks.Clear();   // This might be a source of a bug.  Clear() is being called on a method parameter.
                              // Do I need to use the ref modifier?
    }
    private bool ProcessGridObjectHealth(List<GridObject> objects, float delayDestruction = 0.0f)
    {
        bool returnBool = false;
        for (int i = objects.Count - 1; i > -1; i--)
        {
            Health hp = objects[i].GetComponent<Health>();
            if (hp != null && !hp.HasHP)
            {
                StartCoroutine(DropLootCoroutine(objects[i], objects[i].currentWorldLocation, 0.0f));
                StartCoroutine(GridObjectDestructionCoroutine(objects[i], delayDestruction));
                //ObjectsToDestroyAtStart(objects[i]);

                returnBool = true;
            }
        }

        return returnBool;
    }

    private List<GridBlock> GetGridBlocksInPath(Vector2Int origin, Vector2Int direction)
    {
        List<GridBlock> gridBlockPath = new List<GridBlock>();
        List<Vector2Int> gridLocations = gm.GetGridPath(origin, direction);

        for (int i = 0; i < gridLocations.Count; i++)
        {
            gridBlockPath.Add(gm.FindGridBlockByLocation(gridLocations[i]));
        }

        return gridBlockPath;
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
        //StartCoroutine(DropLootCoroutine(objectToMove, objectToMove.currentWorldLocation));
    }
    private IEnumerator GridObjectDestructionCoroutine(GridObject objectToDestroy, float delay = 0.0f) 
    {
        if (VerboseConsole) Debug.Log("GridObject Destruction Coroutine called.");
        yield return new WaitForSeconds(delay);
        RemoveGridObjectFromPlay(objectToDestroy);
        Destroy(objectToDestroy.gameObject);
        if (VerboseConsole) Debug.Log("GridObject Destruction Coroutine ended.");

        //TODO: Spawn explosion here
    }
    private IEnumerator DropLootCoroutine(GridObject gridObject, Vector3 dropLocation, float delayAppear = 1.0f)
    {
        LootHandler lh = gridObject.gameObject.GetComponent<LootHandler>();
        if (lh != null)
        {
            GameObject lootObjectToDrop = lh.RequestLootDrop(dropLocation, forced: true);

            if (lootObjectToDrop != null) {
                //MeshRenderer renderer = lootObjectToDrop.GetComponent<MeshRenderer>();
                MeshRenderer renderer = lootObjectToDrop.GetComponentInChildren<MeshRenderer>();
                renderer.enabled = false;

                gm.AddObjectToGrid(lootObjectToDrop, gm.WorldToGrid(dropLocation));
                gridObjectsInPlay.Add(lootObjectToDrop.GetComponent<Loot>());

                Rotator lootRotator = lootObjectToDrop.GetComponent<Rotator>();
                lootRotator.enabled = true;
                lootRotator.ApplyRotation("Left");

                yield return new WaitForSeconds(delayAppear);
                renderer.enabled = true;
            }
        }
    }

    public class AnimateTickBehavior
    {
        public enum ExecutionPhase { StartOfHazardPhase, HalfwayHazardPhase, EndOfHazardPhase}
        public enum BehaviorType { Destroy, DropLoot }

        public GridObject gridObject;
        public ExecutionPhase executionPhase;
        public BehaviorType behaviorType;
    }
}