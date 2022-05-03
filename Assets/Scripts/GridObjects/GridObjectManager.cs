using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridObjectManager : MonoBehaviour
{
    [SerializeField] bool VerboseConsole = true;

    //  # INSPECTOR
    [Header("Spawn Management")]
    [SerializeField] int minTicksUntilSpawn = 2;
    [SerializeField] int maxTicksUntilSpawn = 4;

    [HideInInspector] public List<SpawnWave> insertSpawnSequences = new List<SpawnWave>();
    Queue<SpawnRecord> spawnQueue = new Queue<SpawnRecord>();
    

    [Header("GridObject Library")]
    [SerializeField] GridObject[] gridObjectPrefabs;
    List<GridObject> hazards = new List<GridObject>();
    List<GridObject> loot = new List<GridObject>();
    List<GridObject> phenomena = new List<GridObject>();
    List<GridObject> stations = new List<GridObject>();


    public GameObject playerPrefab;

    public GameManager.GameState currentGameState;
    public GamePhase currentGamePhase;
    public bool IsAnimationComplete
    {
        get { return gridObjectAnimationInProgress.Count == 0; }
    }

    // # REFERENCES
    GridManager gridM;
    Player player;
    InputManager inputM;
    PlayerHUD pHUD;

    // #FIELDS
    int currentTick = 0;
    Vector2Int minVector2;
    Vector2Int maxVector2;
    int ticksUntilNewSpawn;

    //List<GridObject> gridObjectsInPlay;           // Object tracking 
    List<GridBlock> collisions;
    Dictionary<GridObject, GridUpdateStep> gridObjectsInPlay;
    List<GridObject> gridObjectAnimationInProgress;


    class GridUpdateStep
    {
        public bool activeInThisPhase;

        public bool canMove;
        public Vector2Int gridOrigin;
        public Vector2Int gridDestination;
        public bool isMoving;
        public bool isDeparting;
        public GridObject collidesWith;

        public bool hasHealth;
        public bool dropsLoot;

        public Vector3 WorldOrigin { get { return new Vector3(gridOrigin.x, gridOrigin.y, 0); } }
        public Vector3 WorldDestination { get { return new Vector3(gridDestination.x, gridDestination.y, 0); } }
        public float AnimateDistance { get { return Vector3.Distance(WorldOrigin, WorldDestination); } }

        public GridUpdateStep()
        {
            activeInThisPhase = false;
            canMove = false;
            gridOrigin = new Vector2Int(99, 99);
            gridDestination = new Vector2Int(99, 99);
            isMoving = false;
            isDeparting = false;
            collidesWith = null;

            hasHealth = true;
            dropsLoot = false;
        }
    }

    //  #INITIALIZATION
    void Awake()
    {
        gridObjectsInPlay = new Dictionary<GridObject, GridUpdateStep>();
        gridObjectAnimationInProgress = new List<GridObject>();
        collisions = new List<GridBlock>();

        pHUD = FindObjectOfType<PlayerHUD>().GetComponent<PlayerHUD>();
    }
    public void Init()
    {
        /*  SUMMARY
         *   - Reference to GridManager
         *   - Cache level boundaries
         *   - Identify next tick requiring a spawn
         *   - Handle Player
         *   - Insert Spawn Sequence data if present
         * 
         */
        gridM = GetComponent<GridManager>();

        minVector2 = new Vector2Int(gridM.BoundaryLeftActual, gridM.BoundaryBottomActual);
        maxVector2 = new Vector2Int(gridM.BoundaryRightActual, gridM.BoundaryTopActual);

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
        currentTick = 1;

        //gridObjectsInPlay.Insert(0, GameObject.FindWithTag("Player").GetComponent<Player>());
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // Some minor debugging
        Debug.LogFormat("Player Instance ID: {0}", player.transform.GetInstanceID().ToString());
        gridObjectsInPlay.Add(player, null);
        //player = gridObjectsInPlay[0].GetComponent<Player>();
        

        // Populate GridObject Lists
        for (int i = 0; i < gridObjectPrefabs.Length; i++)
        {
            if (gridObjectPrefabs[i].spawnRules.spawnCategory == GridObjectType.Hazard)
                hazards.Add(gridObjectPrefabs[i]);

            if (gridObjectPrefabs[i].spawnRules.spawnCategory == GridObjectType.Loot)
                loot.Add(gridObjectPrefabs[i]);

            if (gridObjectPrefabs[i].spawnRules.spawnCategory == GridObjectType.Phenomena)
                phenomena.Add(gridObjectPrefabs[i]);

            if (gridObjectPrefabs[i].spawnRules.spawnCategory == GridObjectType.Station)
                stations.Add(gridObjectPrefabs[i]);
        }

        Debug.LogFormat("Current Game Phase: {0}", currentGamePhase.ToString());
    }
    public void InsertManualSpawnSequence()
    {
        if (insertSpawnSequences.Count > 0)
        {
            for (int i = 0; i < insertSpawnSequences.Count; i++)
            {
                for (int j = 0; j < insertSpawnSequences[i].spawnSteps.Length; j++)
                {
                    spawnQueue.Enqueue(insertSpawnSequences[i].spawnSteps[j]);
                }
            }

            CreateGridObject(spawnQueue.Dequeue());
        }
        else
        {
            AddSpawnStep(SelectGridObject(GridObjectType.Hazard));
            CreateGridObject(spawnQueue.Dequeue());
        }
    }


    //  #SPAWNING
    void AddSpawnStep(GridObject objectForSpawn)
    {
        /*	SUMMARY
         *  -   Randomly selects a spawn location
         *  -   Instantiates SpawnStep ScriptableObject
         *  -   Initializes the new SpawnStep
         *  -   Enqueue the new SpawnStep
         */

        if (VerboseConsole) Debug.Log("GridObjectManager.CreateSpawnStep() called.");

        Vector2Int hazardSpawnLocation = new Vector2Int();

        // DEBUG: Make sure spawn selection is working appropriately
        //if (VerboseConsole) Debug.LogFormat("Array Length: {0}, Random value: {1}", gridObjectPrefabs.Length, gridObjectSelector);

        // Identify an appropriate spawn location
        //        List<Vector2Int> availableSpawns = gm.GetSpawnLocations(gridObjectPrefabs[gridObjectSelector].spawnRules);

        List<Vector2Int> allAvailableSpawns = gridM.GetSpawnLocations(objectForSpawn.spawnRules.spawnRegion);
        List<Vector2Int> finalAvailableSpawns = ResolveSpawns(allAvailableSpawns, objectForSpawn.spawnRules);

        Vector2Int targetLocation = finalAvailableSpawns[Random.Range(0, allAvailableSpawns.Count)];
        hazardSpawnLocation.Set(targetLocation.x, targetLocation.y);

        // Create the SpawnStep
        SpawnRecord newSpawnStep = ScriptableObject.CreateInstance<SpawnRecord>();
        //        newSpawnStep.Init(gridObjectPrefabs[gridObjectSelector], hazardSpawnLocation);
        newSpawnStep.Init(objectForSpawn, hazardSpawnLocation);
        spawnQueue.Enqueue(newSpawnStep);
    }
    GridObject SelectGridObject(GridObjectType type)
    {
        if (type == GridObjectType.Hazard)
        {

            int selector = Random.Range(0, hazards.Count - 1);
            if (VerboseConsole) Debug.Log("Selected a Hazard.");
            return hazards[selector];
        }
        else if (type == GridObjectType.Station)
        {
            int selector = Random.Range(0, stations.Count - 1);
            if (VerboseConsole) Debug.Log("Selected a Station.");
            return stations[selector];
        }
        else if (type == GridObjectType.Phenomena)
        {
            int selector = Random.Range(0, phenomena.Count);
            if (VerboseConsole) Debug.Log("Selected a Phenomenon.");
            return phenomena[selector];
        }
        else if (type == GridObjectType.Loot)
        {
            int selector = Random.Range(0, loot.Count);
            if (VerboseConsole) Debug.Log("Selected a Loot.");
            return loot[selector];
        }
        else
        {
            return null;
        }
    }
    void CreateGridObject(SpawnRecord spawnStep)
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

        GridObject newSpawn = Instantiate(spawnStep.gridObject);

        if (newSpawn.spawnRules.spawnRegion == GridManager.SpawnRule.SpawnRegion.Perimeter)
        {
            string borderName = "";
            if (spawnStep.SpawnLocation.y == gridM.BoundaryBottomActual) borderName = "Bottom";
            else if (spawnStep.SpawnLocation.y == gridM.BoundaryTopActual) borderName = "Top";
            else if (spawnStep.SpawnLocation.x == gridM.BoundaryRightActual) borderName = "Right";
            else if (spawnStep.SpawnLocation.x == gridM.BoundaryLeftActual) borderName = "Left";

            //newSpawn.Init(borderName);
            if (newSpawn is Hazard)
            {
                Hazard newHazard = newSpawn as Hazard;
                newHazard.spawnBorder = borderName;
                newHazard.Init();
            }
            else if (newSpawn is Loot)
            {
                Loot newLoot = newSpawn as Loot;
                newLoot.spawnBorder = borderName;
                newLoot.Init();
            }
            else
            {
                newSpawn.Init();
            }

        }

        PlaceGridObjectInPlay(newSpawn, spawnStep.SpawnLocation);

        if (VerboseConsole) Debug.Log("GridObjectManager.CreateGridObject() completed.");
    }
    public void PlaceGridObjectInPlay(GridObject gridObject, Vector2Int gridLocation, bool placeOnGrid = true)
    {
        if (VerboseConsole) Debug.Log("GridObjectManager.PlaceGridObjectInPlay() called.");

        Vector3 worldLocation = gridM.GridToWorld(gridLocation);

        if (placeOnGrid == false)
        {
            gridObjectsInPlay.Add(gridObject, null);
        }
        else
        {
            gridObject.transform.position = worldLocation;
            gridM.AddObjectToGrid(gridObject.gameObject, gridLocation);
            gridObjectsInPlay.Add(gridObject, null);
        }
    }
    List<Vector2Int> ResolveSpawns(List<Vector2Int> possibleSpawns, GridManager.SpawnRule rule)
    {
        List<Vector2Int> ineligibleSpawns = new List<Vector2Int>();
        if (rule.avoidHazardPaths)
        {
            for (int i = 0; i < gridObjectsInPlay.Count; i++)
            {

                if (gridObjectsInPlay.Keys.ElementAt(i).TryGetComponent<Hazard>(out Hazard h) && gridObjectsInPlay.Keys.ElementAt(i).TryGetComponent<GridMover>(out GridMover mp))
                {
                    Vector2Int gridLocation = gridM.WorldToGrid(h.animateStartWorldLocation);
                    Vector2Int direction = mp.DirectionOnGrid;

                    bool onLeftBoundary = false;
                    bool onRightBoundary = false;
                    bool onTopBoundary = false;
                    bool onBottomBoundary = false;

                    if (gridLocation.x == gridM.BoundaryLeftPlay)
                        onLeftBoundary = true;
                    else if (gridLocation.x == gridM.BoundaryRightPlay)
                        onRightBoundary = true;
                    else if (gridLocation.y == gridM.BoundaryTopPlay)
                        onTopBoundary = true;
                    else if (gridLocation.y == gridM.BoundaryBottomPlay)
                        onBottomBoundary = true;

                    if (direction == Vector2Int.up)
                    {
                        // Disable spawning on opposing GridBlock at boundary
                        Vector2Int oppositeBoundary = new Vector2Int(gridLocation.x, gridM.BoundaryTopActual);
                        ineligibleSpawns.Add(oppositeBoundary);

                        // Remove neighboring GridBlocks along boundary
                        if (onLeftBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y + 1);
                            ineligibleSpawns.Add(neighbor);
                        }

                        if (onRightBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x + 1, gridLocation.y + 1);
                            ineligibleSpawns.Add(neighbor);
                        }
                    }

                    if (direction == Vector2Int.down)
                    {
                        // Disable spawning on opposing GridBlock at boundary
                        Vector2Int oppositeBoundary = new Vector2Int(gridLocation.x, gridM.BoundaryBottomActual);
                        ineligibleSpawns.Add(oppositeBoundary);

                        // Remove neighboring GridBlocks along boundary
                        if (onLeftBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y - 1);
                            ineligibleSpawns.Add(neighbor);
                        }

                        if (onRightBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x + 1, gridLocation.y - 1);
                            ineligibleSpawns.Add(neighbor);
                        }
                    }

                    if (direction == Vector2Int.left)
                    {
                        // Disable spawning on opposing GridBlock at boundary
                        Vector2Int oppositeBoundary = new Vector2Int(gridM.BoundaryLeftActual, gridLocation.y);
                        ineligibleSpawns.Add(oppositeBoundary);

                        // Remove neighboring GridBlocks along boundary
                        if (onTopBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y + 1);
                            ineligibleSpawns.Add(neighbor);
                        }

                        if (onBottomBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y - 1);
                            ineligibleSpawns.Add(neighbor);
                        }
                    }

                    if (direction == Vector2Int.right)
                    {
                        // Disable spawning on opposing GridBlock at boundary
                        Vector2Int oppositeBoundary = new Vector2Int(gridM.BoundaryRightActual, gridLocation.y);
                        ineligibleSpawns.Add(oppositeBoundary);

                        // Remove neighboring GridBlocks along boundary
                        if (onLeftBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y + 1);
                            ineligibleSpawns.Add(neighbor);
                        }

                        if (onRightBoundary)
                        {
                            Vector2Int neighbor = new Vector2Int(gridLocation.x + 1, gridLocation.y + 1);
                            ineligibleSpawns.Add(neighbor);
                        }
                    }
                }
            }

        }

        if (rule.avoidAdjacentToPlayer)
        {
            // Remove Player location and surrounding GridBlocks
            Vector2Int pLocation = gridM.WorldToGrid(player.animateStartWorldLocation);
            for (int x = pLocation.x - 1; x < pLocation.x + 2; x++)
            {
                for (int y = pLocation.y + 1; y > (pLocation.y - 2); y--)
                {
                    Vector2Int area = new Vector2Int(x, y);
                    ineligibleSpawns.Add(area);
                }
            }
        }

        if (rule.avoidShareSpawnLocation)
        {
            for (int i = 0; i < gridObjectsInPlay.Count; i++)
            {
                ineligibleSpawns.Add(gridM.WorldToGrid(gridObjectsInPlay.Keys.ElementAt(i).animateStartWorldLocation));
            }
        }

        //List<Vector2Int> resolvedSpawns = possibleSpawns;
        for (int i = 0; i < possibleSpawns.Count; i++)
        {
            for (int j = 0; j < ineligibleSpawns.Count; j++)
            {
                if (possibleSpawns.Contains(ineligibleSpawns[j]))
                    possibleSpawns.Remove(ineligibleSpawns[j]);
            }
        }
        return possibleSpawns;
    }

    //  #CORE FUNCTIONALITY
    public void OnPlayerActivateModule(Module.UsageData uData)
    {
        if (uData.doesDamage)
        {
            List<GridBlock> targetBlocks = GetGridBlocksInPath(gridM.WorldToGrid(player.animateStartWorldLocation), player.Direction);

            for (int i = 0; i < targetBlocks.Count; i++)
            {
                for (int j = 0; j < targetBlocks[i].objectsOnBlock.Count; j++)
                {
                    Health hp = targetBlocks[i].objectsOnBlock[j].GetComponent<Health>();
                    if (hp != null)
                    {
                        if (uData.dynamicDamage)
                        {
                            int gridBlockDistance = i;
                            int damageAmount = CalculateDamage(gridBlockDistance, uData.baseDamage, uData.damageMultiplier);
                            hp.SubtractHealth(damageAmount);
                        }
                        else
                        {
                            hp.SubtractHealth(uData.baseDamage);
                        }


                        if (!uData.doesPenetrate)
                        {
                            //  #TO-DO: Execute appropriate animation
                            break;
                        }
                    }
                }
            }
            // End animation at end of targetBlocks List
            player.ExecuteAttackAnimation(targetBlocks[targetBlocks.Count - 1]);
        }


        if (uData.doesPlaceObjectInWorld)
        {
            Vector3 playerWorldLocation = gridM.GridToWorld(gridM.FindGridBlockContainingObject(player.gameObject).location);
            GameObject instance = Instantiate(uData.objectToPlaceInWorld, playerWorldLocation, player.transform.rotation);
            instance.GetComponent<GridMover>().SetMovePattern(player.Direction);

            GridObject newObject = instance.GetComponent<GridObject>();
            PlaceGridObjectInPlay(newObject, gridM.WorldToGrid(playerWorldLocation));
        }
    }


    public void NewGridUpdateSteps(bool checkHealth = true, bool checkMove = true, bool checkLoot = true, bool includePlayer = true)
    {
        //int start = includePlayer == true ? 1 : 0;
        //for (int i = start; i < gridObjectsInPlay.Count; i++)
        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            GridUpdateStep newStep = new GridUpdateStep();
            GridObject gridObjectForStep = gridObjectsInPlay.ElementAt(i).Key;

            if (gridObjectForStep.processingPhase == currentGamePhase)
            {
                if (includePlayer == false && gridObjectForStep is Player)
                    newStep.activeInThisPhase = false;
                else
                    newStep.activeInThisPhase = true;
            }
            else
            {
                newStep.activeInThisPhase = false;
            }

            GridBlock _gb = gridM.FindGridBlockContainingObject(gridObjectForStep.gameObject);
            newStep.gridOrigin = _gb.location;

            if (checkHealth)
            {
                if (gridObjectForStep.TryGetComponent<Health>(out Health hp))
                    newStep.hasHealth = hp.HasHP;
                else
                    newStep.hasHealth = true;
            }

            if (checkMove)
            {
                if (gridObjectForStep.TryGetComponent<GridMover>(out GridMover _))
                    newStep.canMove = true;
                else
                    newStep.canMove = false;
            }

            if (checkLoot)
            {
                if (gridObjectForStep.TryGetComponent<LootHandler>(out LootHandler _))
                    newStep.dropsLoot = true;
                else
                    newStep.dropsLoot = false;
            }

            gridObjectsInPlay[gridObjectForStep] = newStep;
        }
    }
    public void LoadGridUpdateSteps()
    {
        /*  DESCRIPTION
         *   - Process a list of GridObjects' data
         *   - Determine one of the following behaviors for the current tick for each object:
         *      ~ Depart the grid
         *      ~ Simply move
         *      ~ FlyBy another object
         *      ~ Nothing
         */

        if (VerboseConsole) Debug.Log("GridObjectManager.FillBoardUpdateSteps called.");

        Vector2Int[] allOriginGridLocations = new Vector2Int[gridObjectsInPlay.Count];          // Fly-By Tracker 1
        Vector2Int[] allDestinationGridLocations = new Vector2Int[gridObjectsInPlay.Count];     // Fly-By Tracker 2

        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            allOriginGridLocations[i].Set(99, 99);
            allDestinationGridLocations[i].Set(99, 99);
        }

        if (VerboseConsole) Debug.Log("Processing grid object movement.");
        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            KeyValuePair<GridObject, GridUpdateStep> kvp = gridObjectsInPlay.ElementAt(i);
            if (kvp.Value.activeInThisPhase == false)
                continue;

            if (kvp.Value.canMove)
            {
                GridMover mp = kvp.Key.GetComponent<GridMover>();
                mp.OnTickUpdate();

                if (mp.CanMoveThisTurn)
                {
                    kvp.Value.gridDestination = kvp.Value.gridOrigin + mp.DirectionOnGrid;

                    allOriginGridLocations[i] = kvp.Value.gridOrigin;                                           // Maintain index of objects requiring additional processing
                    allDestinationGridLocations[i] = kvp.Value.gridDestination;

                    kvp.Value.isMoving = true;

                    if (!gridM.CheckIfGridBlockInBounds(kvp.Value.gridDestination))
                    {
                        kvp.Value.isDeparting = true;
                    }
                }
            }
        }

        if (VerboseConsole) Debug.Log("Fly-By detection starting.");
        for (int i = 0; i < allOriginGridLocations.Length; i++)
        {
            for (int j = 0; j < allDestinationGridLocations.Length; j++)
            {
                if (gridObjectsInPlay.Count > 1 && i == j)
                {
                    continue;
                }
                else if (allOriginGridLocations[i].x == 99)
                {
                    continue;
                }
                else if (allDestinationGridLocations[j].x == 99)
                {
                    continue;
                }
                else if (allOriginGridLocations[i] == allDestinationGridLocations[j] && allOriginGridLocations[j] == allDestinationGridLocations[i])
                {
                    gridObjectsInPlay.ElementAt(i).Value.collidesWith = gridObjectsInPlay.ElementAt(j).Key;
                }
            }
        }

        if (VerboseConsole) Debug.Log("Begining health check");
    }
    public void RunGridUpdate()
    {
        if (VerboseConsole) Debug.Log("GridObjectManager.UpdateBoardData() called.");

        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            KeyValuePair<GridObject, GridUpdateStep> kvp = gridObjectsInPlay.ElementAt(i);

            if (kvp.Value.activeInThisPhase)
            {
                if (kvp.Key.CurrentMode == GridObject.Mode.Spawn)
                    kvp.Key.SetGamePlayMode(GridObject.Mode.Play);

                if (kvp.Value.isDeparting)
                {
                    // Handle departing
                }

                if (kvp.Value.collidesWith != null)
                {
                    GridObject iGridO = kvp.Key;
                    //GridObject jGridO = gridObjectsInPlay[gridObjectsInPlay.IndexOf(step.collidesWith)];
                    GridObject jGridO = kvp.Value.collidesWith;

                    iGridO.TryGetComponent<Health>(out Health iHP);
                    //iGridO.TryGetComponent<ContactDamage>(out ContactDamage iDamage);         // Might need, requires testing
                    //jGridO.TryGetComponent<Health>(out Health jHP);                           // Might need, requires testing
                    jGridO.TryGetComponent<ContactDamage>(out ContactDamage jDamage);

                    iHP.SubtractHealth(jDamage.DamageAmount);
                    //jHP.SubtractHealth(iDamage.DamageAmount);                                 // Might need, requires testing
                }

                // Eligible to move
                if (kvp.Value.canMove)
                {
                    //gridM.RemoveObjectFromGrid(gridObjectsInPlay[i].gameObject, step.gridOrigin);
                    gridM.RemoveObjectFromGrid(kvp.Key.gameObject);
                    //gridM.AddObjectToGrid(gridObjectsInPlay[i].gameObject, step.gridDestination);
                    gridM.AddObjectToGrid(kvp.Key.gameObject, kvp.Value.gridDestination);
                }

                if (!IsLocationInCollisionTracker(kvp.Value.gridDestination))
                    collisions.Add(gridM.FindGridBlockByLocation(kvp.Value.gridDestination));
            }
        }
    }
    public void AnimateMovement()
    {
        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            KeyValuePair<GridObject, GridUpdateStep> kvp = gridObjectsInPlay.ElementAt(i);
            if (kvp.Value.isMoving)
            {
                StartCoroutine(AnimateMoveCoroutine(kvp.Key, kvp.Value.AnimateDistance, kvp.Value.WorldOrigin, kvp.Value.WorldDestination));
            }
        }
    }
    public void RemoveDepartedObjects()
    {
        for (int i = gridObjectsInPlay.Count; i < 0; i--)
        {
            KeyValuePair<GridObject, GridUpdateStep> kvp = gridObjectsInPlay.ElementAt(i);
            if (kvp.Value.isDeparting)
            {
                GameObject gameObjectToRemove = kvp.Key.gameObject;
                Vector2Int gridPosition = kvp.Value.gridDestination;

                gridM.RemoveObjectFromGrid(gameObjectToRemove);
                gridObjectsInPlay.Remove(kvp.Key);

                Destroy(gameObjectToRemove);
            }
        }
    }
    public void RemoveDeadObjectsAndDropLoot()
    {
        List<GridObject> objectsToDestroy = new List<GridObject>();
        List<GridObject> lootToPutIntoPlay = new List<GridObject>();
        List<Vector2Int> locationsToPlaceLoot = new List<Vector2Int>();

        for (int i = gridObjectsInPlay.Count - 1; i >= 0; i--)
        {
            KeyValuePair<GridObject, GridUpdateStep> kvp = gridObjectsInPlay.ElementAt(i);
            if (kvp.Value.hasHealth == false)
            {
                objectsToDestroy.Add(kvp.Key);
                if (kvp.Value.dropsLoot)
                {
                    LootHandler lh = kvp.Key.GetComponent<LootHandler>();
                    GameObject go = lh.RequestLootDrop(kvp.Value.WorldOrigin);

                    if (go != null)
                    {
                        lootToPutIntoPlay.Add(go.GetComponent<GridObject>());
                        locationsToPlaceLoot.Add(kvp.Value.gridOrigin);
                    }
                }
            }
        }

        /*  DEPRECATED - USED WHEN gridObjectsInPlay was <List>
        for (int i = gridObjectsInPlay.Count; i <= 0; i--)
        {
            for (int j = 0; j < objectsToDestroy.Count; j++)
            {
                if (gridObjectsInPlay[i] == objectsToDestroy[j])
                {
                    GameObject dead = gridObjectsInPlay[i].gameObject;
                    gridObjectsInPlay.RemoveAt(i);
                    Destroy(dead);
                }
            }
        }
        */

        for (int i = 0; i < objectsToDestroy.Count; i++)
        {
            gridM.RemoveObjectFromGrid(objectsToDestroy[i].gameObject);
            gridObjectsInPlay.Remove(objectsToDestroy[i]);
            Destroy(objectsToDestroy[i].gameObject);
        }
        objectsToDestroy.Clear();

        for (int i = 0; i < lootToPutIntoPlay.Count; i++)
        {
            PlaceGridObjectInPlay(lootToPutIntoPlay[i], locationsToPlaceLoot[i]);
        }
    }
    public void SpawnGridObjects()
    {
        if (ticksUntilNewSpawn == 0 || gridObjectsInPlay.Count == 0)
        {
            if (spawnQueue.Count > 0)
            {
                CreateGridObject(spawnQueue.Dequeue());
            }
            else
            {
                if (Random.Range(0, 10) > 7)    // 20% chance
                {
                    AddSpawnStep(SelectGridObject(GridObjectType.Loot));
                    CreateGridObject(spawnQueue.Dequeue());
                }
                else
                {
                    AddSpawnStep(SelectGridObject(GridObjectType.Hazard));
                    CreateGridObject(spawnQueue.Dequeue());
                }
            }

            ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
        }
    }

    public void ResolveCollisionsOnGridBlocks()
    {
        // i = GridBlock iterator
        // j = objects on GridBlock iterator 1
        // k = objects on GridBlock iterator 2 (j+1)

        for (int i = 0; i < collisions.Count; i++)
        {
            if (collisions[i].objectsOnBlock.Count > 1)
            {
                Debug.Log("Processing collision on " + collisions[i].location.ToString());

                for (int j = 0; j < collisions[i].objectsOnBlock.Count; j++)
                {
                    GridObject jGridObject = collisions[i].objectsOnBlock[j].GetComponent<GridObject>();

                    for (int k = 1 + j; k < collisions[i].objectsOnBlock.Count; k++)
                    {
                        GridObject kGridObject = collisions[i].objectsOnBlock[k].GetComponent<GridObject>();

                        bool jIsPlayer = jGridObject is Player;
                        bool kIsPlayer = kGridObject is Player;

                        bool jIsStation = jGridObject is Station;
                        bool kIsStation = kGridObject is Station;

                        bool jIsLoot = jGridObject is Loot;
                        bool kIsLoot = kGridObject is Loot;

                        bool jHasHP = jGridObject.gameObject.TryGetComponent<Health>(out Health jHealth);
                        bool kHasHP = kGridObject.gameObject.TryGetComponent<Health>(out Health kHealth);

                        bool jDoesDamage = jGridObject.gameObject.TryGetComponent<ContactDamage>(out ContactDamage jDamage);
                        bool kDoesDamage = kGridObject.gameObject.TryGetComponent<ContactDamage>(out ContactDamage kDamage);

                        bool jDoesRepair = jGridObject.TryGetComponent<ContactRepair>(out ContactRepair jRepair);
                        bool kDoesRepair = kGridObject.TryGetComponent<ContactRepair>(out ContactRepair kRepair);

                        bool jDoesSupply = jGridObject.TryGetComponent<ContactSupply>(out ContactSupply jSupply);
                        bool kDoesSupply = kGridObject.TryGetComponent<ContactSupply>(out ContactSupply kSupply);

                        bool jDoesFuel = jGridObject.TryGetComponent<ContactFuel>(out ContactFuel jFuel);
                        bool kDoesFuel = kGridObject.TryGetComponent<ContactFuel>(out ContactFuel kFuel);

                        bool jDoesSlow = jGridObject.TryGetComponent<ContactSlow>(out ContactSlow jSlow);
                        bool kDoesSlow = kGridObject.TryGetComponent<ContactSlow>(out ContactSlow kSlow);

                        if (jHasHP && kDoesDamage)
                        {
                            if (!jIsStation)
                                jHealth.SubtractHealth(kDamage.DamageAmount);
                        }

                        if (kHasHP && jDoesDamage)
                        {
                            if (!kIsStation)
                                kHealth.SubtractHealth(jDamage.DamageAmount);
                        }

                        if ((jIsPlayer && kDoesSupply) || (kIsPlayer && jDoesSupply))
                        {
                            if (jIsPlayer)
                            {
                                Player p = jGridObject as Player;
                                ContactSupply cs = kGridObject.GetComponent<ContactSupply>();

                                if (kIsStation)
                                {
                                    Station s = kGridObject as Station;
                                    bool successfulDock = s.Dock();

                                    if (successfulDock)
                                    {
                                        p.AcceptAmmo(cs.weaponType, cs.supplyAmount);
                                        s.BeginReplenish();
                                    }
                                }
                                else
                                {
                                    p.AcceptAmmo(cs.weaponType, cs.supplyAmount);
                                    cs.ConsumeSupply();
                                }

                                if (kIsLoot)
                                    kHealth.SubtractHealth(kHealth.CurrentHP);

                                if (VerboseConsole)
                                    Debug.LogFormat("Player is picking up {0} of {1} ammo.", cs.supplyAmount.ToString(), cs.weaponType.ToString());
                            }
                            else if (kIsPlayer)
                            {
                                Player p = kGridObject as Player;
                                ContactSupply cs = jGridObject.GetComponent<ContactSupply>();

                                if (jIsStation)
                                {
                                    Station s = jGridObject as Station;
                                    bool successfulDock = s.Dock();

                                    if (successfulDock)
                                    {
                                        p.AcceptAmmo(cs.weaponType, cs.supplyAmount);
                                        s.BeginReplenish();
                                    }
                                }
                                else
                                {
                                    p.AcceptAmmo(cs.weaponType, cs.supplyAmount);
                                    cs.ConsumeSupply();
                                }

                                if (jIsLoot)
                                    jHealth.SubtractHealth(jHealth.CurrentHP);

                                if (VerboseConsole)
                                    Debug.LogFormat("Player is picking up {0} of {1} ammo.", cs.supplyAmount.ToString(), cs.weaponType.ToString());
                            }
                        }

                        if ((jIsPlayer && kDoesFuel) || (kIsPlayer && jDoesFuel))
                        {
                            if (jIsPlayer)
                            {
                                Player p = jGridObject as Player;

                                if (kIsStation)
                                {
                                    Station s = kGridObject as Station;
                                    bool successfulDock = s.Dock();

                                    if (successfulDock)
                                    {
                                        p.AcceptFuel(kFuel.fuelAmount);
                                        s.BeginReplenish();
                                    }
                                    if (VerboseConsole)
                                        Debug.LogFormat("{0} jump fuel retrieved.", kFuel.fuelAmount);
                                }
                                else
                                {
                                    p.AcceptFuel(kFuel.fuelAmount);

                                    if (VerboseConsole)
                                        Debug.LogFormat("{0} jump fuel retrieved.", kFuel.fuelAmount);
                                }
                            }
                            else if (kIsPlayer)
                            {
                                Player p = kGridObject as Player;

                                if (jIsStation)
                                {
                                    Station s = jGridObject as Station;
                                    bool successfulDock = s.Dock();

                                    if (successfulDock)
                                    {
                                        p.AcceptFuel(jFuel.fuelAmount);
                                        s.BeginReplenish();

                                        if (VerboseConsole)
                                            Debug.LogFormat("{0} jump fuel retrieved.", jFuel.fuelAmount);
                                    }
                                }
                                else
                                {
                                    p.AcceptFuel(jFuel.fuelAmount);

                                    if (VerboseConsole)
                                        Debug.LogFormat("{0} jump fuel retrieved.", jFuel.fuelAmount);
                                }
                            }
                        }

                        if ((jIsPlayer && kDoesRepair) || (kIsPlayer && jDoesRepair))
                        {
                            if (jIsPlayer)
                            {
                                Player p = jGridObject as Player;

                                if (kIsStation)
                                {
                                    Station s = kGridObject as Station;
                                    bool successfulDock = s.Dock();

                                    if (successfulDock)
                                    {
                                        jHealth.AddHealth(kRepair.repairAmount);
                                        s.BeginReplenish();
                                    }
                                }
                                else
                                {
                                    jHealth.AddHealth(kRepair.repairAmount);
                                    kRepair.ConsumeRepair();
                                }
                            }
                            else if (kIsPlayer)
                            {
                                Player p = kGridObject as Player;

                                if (jIsStation)
                                {
                                    Station s = jGridObject as Station;
                                    bool successfulDock = s.Dock();

                                    if (successfulDock)
                                    {
                                        kHealth.AddHealth(jRepair.repairAmount);

                                        s.BeginReplenish();
                                    }
                                }
                                else
                                {
                                    kHealth.AddHealth(jRepair.repairAmount);
                                    jRepair.ConsumeRepair();
                                }
                            }
                        }

                        if (jDoesSlow || kDoesSlow)
                        {
                            if (jDoesSlow && kGridObject.TryGetComponent<GridMover>(out GridMover kMove) && kMove.IsSlowed)
                                kMove.ApplySlow(jSlow.tickDelayAmount);

                            if (kDoesSlow && jGridObject.TryGetComponent<GridMover>(out GridMover jMove) && jMove.IsSlowed)
                                jMove.ApplySlow(kSlow.tickDelayAmount);
                        }
                    }
                }
            }
        }

        collisions.Clear();
    }
    bool IsLocationInCollisionTracker(Vector2Int gridLocation)
    {
        for (int i = 0; i < collisions.Count; i++)
        {
            if (collisions[i].location != gridLocation)
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



    // CHANGING LEVELS
    public void ClearLevel()
    {
        if (VerboseConsole)
            Debug.Log("Preparing for next level. Removing all GridObjects in play.");

        for (int i = gridObjectsInPlay.Count - 1; i > 0; i--)
        {
            //RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
            GridObject gridObject = gridObjectsInPlay.Keys.ElementAt(i);
            GameObject gameObject = gridObject.gameObject;
            gridObjectsInPlay.Remove(gridObject);
            Destroy(gameObject);
        }

        spawnQueue.Clear();
        //gridObjectsInPlay[0].gameObject.SetActive(false);       //Disable Player 
        //StartCoroutine(player.AnimateNextLevel());

        if (VerboseConsole)
            Debug.Log("Preparations for next level complete.");
    }
    public void NextLevel(int phenomenaRequired, int stationsRequired)
    {
        // For each phenomenaRequired, should randomly select one 
        for (int i = 0; i < phenomenaRequired; i++)
        {
            AddSpawnStep(SelectGridObject(GridObjectType.Phenomena));
            CreateGridObject(spawnQueue.Dequeue());
        }

        for (int i = 0; i < stationsRequired; i++)
        {
            AddSpawnStep(SelectGridObject(GridObjectType.Station));
            CreateGridObject(spawnQueue.Dequeue());
        }

        // For each stationRequired, should randomly select one
        // Create SpawnSteps

    }
    public void ArrivePlayer()
    {
        //#TODO
        // Need to add something that will allow for a Player to spawn somewhere other than 0, 0

        Vector2Int arriveLocation = new Vector2Int(0, 0);

        if (VerboseConsole) Debug.Log("Player Jump successful.  Adding Player to new Grid.");

        if (player.spawnRules.spawnRegion == GridManager.SpawnRule.SpawnRegion.Center)
        {
            // Can put some new arrival logic here    
        }

        gridM.AddObjectToGrid(player.gameObject, arriveLocation);
        //player.animateStartWorldLocation = gridM.GridToWorld(arriveLocation);
        //player.animateEndWorldLocation = gridM.GridToWorld(arriveLocation);
        player.transform.position = new Vector3(arriveLocation.x, arriveLocation.y, 0);

        if (VerboseConsole) Debug.Log("Player successfully added to Grid.");
    }


    // UTILITY METHODS
    int CalculateDamage(int gridBlockDistance, int baseDamage, float damageMultiplier, bool verboseConsole = false)
    {
        /*  NOTE ON gridBlockDistance
        *    If the Player occupies a GridBlock that is immediately adjacent to the target, the distance = 0
        *    If there is one GridBlock in between the Player and the target, the distance = 1
        *    If there are two GridBlocks in between the Player and the target, the distance = 2
        *    If there are three GridBlocks in between the Player and the target, the distance = 3
        */

        if (verboseConsole)
        {
            Debug.LogFormat("Grid Block Distance received: {0}", gridBlockDistance);
            Debug.LogFormat("Grid Modifier: {0}", damageMultiplier);
            Debug.LogFormat("Base Damage: {0}", baseDamage);
        }

        float modDamageValue = Mathf.Pow((float)damageMultiplier, (float)gridBlockDistance);
        if (verboseConsole)
            Debug.LogFormat("Damage modifier value: {0}", modDamageValue.ToString());

        float newDamageValue = baseDamage * modDamageValue;
        if (verboseConsole)
            Debug.LogFormat("Damage modifier value: {0}", newDamageValue.ToString());

        return (int)newDamageValue;
    }
    void UpdateStations()
    {

        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            if (gridObjectsInPlay.Keys.ElementAt(i) is Station)
            {
                Station s = gridObjectsInPlay.Keys.ElementAt(i) as Station;
                s.OnTickUpdate();
            }
        }
    }
    List<GridBlock> GetGridBlocksInPath(Vector2Int origin, Vector2Int direction)
    {
        List<GridBlock> gridBlockPath = new List<GridBlock>();
        List<Vector2Int> gridLocations = gridM.GetGridPath(origin, direction);

        for (int i = 0; i < gridLocations.Count; i++)
        {
            gridBlockPath.Add(gridM.FindGridBlockByLocation(gridLocations[i]));
        }

        return gridBlockPath;
    }


    // ANIMATION & MOVEMENT COROUTINES
    IEnumerator AnimateMoveCoroutine(GridObject objectToMove, float Distance, Vector3 startLocation, Vector3 endLocation)
    {
        gridObjectAnimationInProgress.Add(objectToMove);

        float startTime = Time.time;
        float percentTraveled = 0.0f;

        Debug.LogFormat("Instance of ID of object being move animated: {0}", objectToMove.transform.GetInstanceID().ToString());

        while (percentTraveled <= 1.0)
        {
            Debug.LogFormat("Beginning to move {0} from {1} to {2}.", objectToMove.gameObject.name, startLocation.ToString(), endLocation.ToString());
            
            float traveled = (Time.time - startTime) * 1.0f;
            percentTraveled = traveled / Distance;
            objectToMove.transform.position = Vector3.Lerp(startLocation, endLocation, Mathf.SmoothStep(0, 1, percentTraveled));

            Debug.LogFormat("{0} has traveled {1}% of the distance.", objectToMove.gameObject.name, percentTraveled.ToString());
            yield return null;
        }

        gridObjectAnimationInProgress.Remove(objectToMove);
        Debug.Log("Move animation complete.");
    }
    IEnumerator DropLootCoroutine(GridObject gridObject, Vector3 dropLocation, float delayAppear = 1.0f)
    {
        LootHandler lh = gridObject.gameObject.GetComponent<LootHandler>();
        if (lh != null)
        {
            GameObject lootObjectToDrop = lh.RequestLootDrop(dropLocation, forced: true);

            if (lootObjectToDrop != null)
            {
                MeshRenderer renderer = lootObjectToDrop.GetComponentInChildren<MeshRenderer>();
                renderer.enabled = false;

                gridM.AddObjectToGrid(lootObjectToDrop, gridM.WorldToGrid(dropLocation));
                //gridObjectsInPlay.Add(lootObjectToDrop.GetComponent<Loot>());
                gridObjectsInPlay.Add(gridObject, null);

                Rotator lootRotator = lootObjectToDrop.GetComponent<Rotator>();
                lootRotator.enabled = true;
                lootRotator.ApplyRotation("Left");

                yield return new WaitForSeconds(delayAppear);
                renderer.enabled = true;
            }
        }
    }
}
public enum GridObjectType
{
    Hazard = 1,
    Phenomena = 2,
    Station = 3,
    Loot = 4
};


// METHODS UNDERGOING DEPRECATION
/*
public void RemoveGridObjectFromPlay(GridObject objectToRemove, bool removeFromGrid = true)
{
    GameObject gameObjectToRemove = objectToRemove.gameObject;
    Vector2Int gridPosition = gridM.FindGridBlockContainingObject(gameObjectToRemove).location;

    if (removeFromGrid) gridM.RemoveObjectFromGrid(gameObjectToRemove, gridPosition);
    gridObjectsInPlay.Remove(objectToRemove);
}
*/
/*
public float OnManagerTickUpdate()
{
    bool moveOccurredThisTick = true;
    float moveDurationSeconds = 1.0f;

    bool gridObjectDestroyedThisTick = false;
    float destroyDurationSeconds = 2.0f;

    float delayTime = 0.0f;

    List<GridObject> objectProcessing = new List<GridObject>();
    List<GridBlock> potentialBlockCollisions = new List<GridBlock>();

    CheckHealth(gridObjectsInPlay);
    UpdateStations();

    // Process GridObject behavior for current Tick
    for (int i = 0; i < gridObjectsInPlay.Count; i++)
    {
        if (gridObjectsInPlay[i].ProcessingPhase == GamePhase.Manager)
        {
            Health hp = gridObjectsInPlay[i].GetComponent<Health>();

            if (hp != null && hp.HasHP)
                objectProcessing.Add(gridObjectsInPlay[i]);
        }

    }
    potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);

    gridObjectDestroyedThisTick = CheckHealth(gridObjectsInPlay, 1.0f);


    // SPAWNING
    if (ticksUntilNewSpawn == 0 || gridObjectsInPlay.Count == 0)
    {
        if (spawnQueue.Count > 0)
        {
            CreateGridObject(spawnQueue.Dequeue());
        }
        else
        {
            if (Random.Range(0, 10) > 7)    // 20% chance
            {
                AddSpawnStep(SelectGridObject(GridObjectType.Loot));
                CreateGridObject(spawnQueue.Dequeue());
            }
            else
            {
                AddSpawnStep(SelectGridObject(GridObjectType.Hazard));
                CreateGridObject(spawnQueue.Dequeue());
            }
        }

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
    }

    currentTick++;
    ticksUntilNewSpawn--;

    ProcessCollisionsOnGridBlock(potentialBlockCollisions);
    CheckHealth(gridObjectsInPlay, 1.0f);

    if (moveOccurredThisTick) delayTime += moveDurationSeconds;
    if (gridObjectDestroyedThisTick) delayTime += destroyDurationSeconds;
    return delayTime;
}
*/
/*
public bool CheckHealth(List<GridObject> objects, float delayAnimation = 0.0f)
{
    bool returnBool = false;
    for (int i = objects.Count - 1; i > -1; i--)
    {
        Health hp = objects[i].GetComponent<Health>();
        if (hp != null && !hp.HasHP)
        {
            StartCoroutine(DropLootCoroutine(objects[i], objects[i].animateEndWorldLocation, delayAnimation));    // <- objects[i].currentWorldLocation
            StartCoroutine(GridObjectDestructionCoroutine(objects[i], delayAnimation));

            returnBool = true;
        }
    }

    return returnBool;
}
*/
/*
void MoveGridObjectsForTick(List<GridObject> objects)
{
    /*  DESCRIPTION
     *   - Process a list of GridObjects' data
     *   - Determine one of the following behaviors for the current tick for each object:
     *      ~ Depart the grid
     *      ~ Simply move
     *      ~ FlyBy another object
     *      ~ Nothing
     *

    Vector2Int[] allOriginGridLocations = new Vector2Int[objects.Count];
    Vector2Int[] allDestinationGridLocations = new Vector2Int[objects.Count];

    List<GridBlock> collisionTestBlocks = new List<GridBlock>();        // Tracker for GridBlock collisions

    for (int i = 0; i < objects.Count; i++)
    {
        allOriginGridLocations[i].Set(99, 99);
        allDestinationGridLocations[i].Set(99, 99);
    }

    // Process movement data
    for (int i = 0; i < objects.Count; i++)
    {
        if (objects[i].TryGetComponent<MovePattern>(out MovePattern mp))
        {
            mp.OnTickUpdate();

            if (mp.CanMoveThisTurn)
            {
                Vector2Int currentLocation = gridM.WorldToGrid(objects[i].animateStartWorldLocation);
                Vector2Int destinationLocation = currentLocation + mp.DirectionOnGrid;

                if (gridM.CheckIfGridBlockInBounds(destinationLocation))
                {
                    // Move or FlyBy
                    allOriginGridLocations[i] = currentLocation;            // Maintain index of objects requiring additional processing
                    allDestinationGridLocations[i] = destinationLocation;
                }
                else
                {
                    // Depart
                    objects[i].IsLeavingGrid = true;
                }
            }
        }
    }

    if (VerboseConsole) Debug.Log("Fly-By detection starting.");
    for (int i = 0; i < allOriginGridLocations.Length; i++)
    {
        for (int j = 0; j < allDestinationGridLocations.Length; j++)
        {
            if (objects.Count > 1 && i == j)
            {
                continue;
            }
            else if (allOriginGridLocations[i].x == 99)
            {
                continue;
            }
            else if (allDestinationGridLocations[j].x == 99)
            {
                continue;
            }
            else if (allOriginGridLocations[i] == allDestinationGridLocations[j] && allOriginGridLocations[j] == allDestinationGridLocations[i])
            {
                Health hp = objects[i].GetComponent<Health>();
                hp.SubtractHealth(objects[j].GetComponent<ContactDamage>().DamageAmount);
            }
        }
    }

    if (VerboseConsole) Debug.Log("Moving GridObjects.");

    for (int i = 0; i < objects.Count; i++)
    {
        if (objects[i].CurrentMode == GridObject.Mode.Spawn)
        {
            objects[i].SetGamePlayMode(GridObject.Mode.Play);
        }

        if (objects[i].TryGetComponent<MovePattern>(out MovePattern mp))
        {
            Vector2Int currentLocation = gridM.WorldToGrid(objects[i].animateStartWorldLocation);
            Vector2Int destinationLocation = currentLocation + mp.DirectionOnGrid;

            if (mp.CanMoveThisTurn)
            {
                if (objects[i].IsLeavingGrid == false && objects[i].GetComponent<Health>().HasHP)
                {
                    // Eligible to move
                    gridM.RemoveObjectFromGrid(objects[i].gameObject, currentLocation);
                    gridM.AddObjectToGrid(objects[i].gameObject, destinationLocation);

                    objects[i].animateEndWorldLocation = gridM.GridToWorld(destinationLocation);

                    if (!IsLocationInCollisionTracker(destinationLocation, collisionTestBlocks))
                    {
                        collisionTestBlocks.Add(gridM.FindGridBlockByLocation(destinationLocation));
                    }

                    StartCoroutine(GridObjectMovementCoroutine(objects[i], 1.0f));
                }
                else
                {   // Departing
                    objects[i].animateEndWorldLocation = gridM.GridToWorld(destinationLocation);

                    StartCoroutine(GridObjectMovementCoroutine(objects[i], 1.0f));
                    StartCoroutine(GridObjectDestructionCoroutine(objects[i], 1.1f));
                }
            }
        }
    }

    return collisionTestBlocks;
}
*/
/*
IEnumerator GridObjectMovementCoroutine(GridObject objectToMove, float travelLength = 1.0f)
{
    float startTime = Time.time;
    float percentTraveled = 0.0f;

    while (percentTraveled <= travelLength)
    {
        float traveled = (Time.time - startTime) * 1.0f;
        percentTraveled = traveled / objectToMove.Distance;
        objectToMove.transform.position = Vector3.Lerp(objectToMove.animateStartWorldLocation, objectToMove.animateEndWorldLocation, Mathf.SmoothStep(0, 1, percentTraveled));

        yield return null;
    }

    objectToMove.animateStartWorldLocation = objectToMove.animateEndWorldLocation;
    //StartCoroutine(DropLootCoroutine(objectToMove, objectToMove.currentWorldLocation));
}
*/
/*
IEnumerator GridObjectDestructionCoroutine(GridObject objectToDestroy, float delay = 0.0f)
{
    if (VerboseConsole) Debug.Log("GridObject Destruction Coroutine called.");
    yield return new WaitForSeconds(delay);
    RemoveGridObjectFromPlay(objectToDestroy);
    Destroy(objectToDestroy.gameObject);
    if (VerboseConsole) Debug.Log("GridObject Destruction Coroutine ended.");

    //TODO: Spawn explosion here
}
*/
/*
public void OnPlayerMove()
{
    List<GridObject> objectProcessing = new List<GridObject>();
    List<GridBlock> potentialBlockCollisions = new List<GridBlock>();

    for (int i = 0; i < gridObjectsInPlay.Count; i++)
    {
        //if (gridObjectsInPlay[i].processingPhase == currentGameState)
            objectProcessing.Add(gridObjectsInPlay[i]);
    }
    player.OnTickUpdate();
    //potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);
    //ProcessCollisionsOnGridBlock(potentialBlockCollisions);
}
*/

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

public class AnimateTickBehavior
{
    public enum ExecutionPhase { StartOfHazardPhase, HalfwayHazardPhase, EndOfHazardPhase}
    public enum BehaviorType { Destroy, DropLoot }

    public GridObject gridObject;
    public ExecutionPhase executionPhase;
    public BehaviorType behaviorType;
}
*/
/*
public float OnTickUpdate(GamePhase phase)  #OnTickUpdate_OLD
    {
        /*  STEPS
         * 
         *  1) GridObject Health Check
         *  2) Move hazards
         *  3) Detect Fly-Bys
         *  4) Hazard Health Check
         *  5) Collisions on a GridBlock
         *

// TICK DURATION
bool moveOccurredThisTick = true;
float moveDurationSeconds = 1.0f;

bool gridObjectDestroyedThisTick = false;
float destroyDurationSeconds = 2.0f;

float delayTime = 0.0f;

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
    }
    else
    {
        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            if (gridObjectsInPlay[i].ProcessingPhase == GamePhase.Player && !gridObjectsInPlay[i].CompareTag("Player"))
                objectProcessing.Add(gridObjectsInPlay[i]);
        }

        Weapon playerWeapon = player.SelectedWeapon;
        // GridObjectManager needs to know the Module

        //if (playerWeapon.currentAmmunition > 0)

        List<GridBlock> targetBlocks = GetGridBlocksInPath(gm.WorldToGrid(player.currentWorldLocation), player.Direction);

        if (playerWeapon.RequiresGridPlacement)
        {
            GameObject weaponInstance = Instantiate(playerWeapon.WeaponPrefab, player.currentWorldLocation, player.transform.rotation);
            weaponInstance.GetComponent<MovePattern>().SetMovePattern(player.Direction);

            GridObject weaponObject = weaponInstance.GetComponent<GridObject>();
            AddObjectToGrid(weaponObject, gm.WorldToGrid(player.currentWorldLocation));
            weaponObject.targetWorldLocation = weaponObject.currentWorldLocation + gm.GridToWorld(player.Direction);
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
                        //hp.SubtractHealth(player.SelectedWeapon.Damage);
                        int gridBlockDistance = i;
                        hp.SubtractHealth(player.SelectedWeapon.CalculateDamage(gridBlockDistance));

                        if (!player.SelectedWeapon.DoesPenetrate)
                        {
                            player.ExecuteAttackAnimation(targetBlocks[i]);
                            break;
                        }
                    }
                }
            }
            // End animation at end of targetBlocks List
            player.ExecuteAttackAnimation(targetBlocks[targetBlocks.Count - 1]);
        }

        //playerWeapon.SubtractAmmo();
        //StartCoroutine(player.UpdateUICoroutine());

    }

    potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);
    player.IsAttackingThisTick = false;

    pHUD.ModuleActivated -= ProcessModuleOnPlayer;
}

if (phase == GamePhase.Manager)
{
    CheckHealth(gridObjectsInPlay);
    UpdateStations();

    // Process GridObject behavior for current Tick
    for (int i = 0; i < gridObjectsInPlay.Count; i++)
    {
        if (gridObjectsInPlay[i].ProcessingPhase == GamePhase.Manager)
        {
            Health hp = gridObjectsInPlay[i].GetComponent<Health>();

            if (hp != null && hp.HasHP)
                objectProcessing.Add(gridObjectsInPlay[i]);
        }

    }
    potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);

    gridObjectDestroyedThisTick = CheckHealth(gridObjectsInPlay, 1.0f);


    // SPAWNING
    if (ticksUntilNewSpawn == 0 || gridObjectsInPlay.Count == 0)
    {
        if (spawnQueue.Count > 0)
        {
            CreateGridObject(spawnQueue.Dequeue());
        }
        else
        {
            if (Random.Range(0, 10) > 7)    // 20% chance
            {
                AddSpawnStep(SelectGridObject(GridObjectType.Loot));
                CreateGridObject(spawnQueue.Dequeue());
            }
            else
            {
                AddSpawnStep(SelectGridObject(GridObjectType.Hazard));
                CreateGridObject(spawnQueue.Dequeue());
            }
        }

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
    }

    currentTick++;
    ticksUntilNewSpawn--;
}

ProcessCollisionsOnGridBlock(potentialBlockCollisions);
CheckHealth(gridObjectsInPlay, 1.0f);

if (moveOccurredThisTick) delayTime += moveDurationSeconds;
if (gridObjectDestroyedThisTick) delayTime += destroyDurationSeconds;
return delayTime;
    }
 */