using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class GridObjectManager : MonoBehaviour
{
    //  # INSPECTOR
    [TitleGroup("DEBUGGING")][SerializeField] bool VerboseDebugging;
    [TitleGroup("DEBUGGING")][SerializeField] bool EnableInitializationDebugging;
    [TitleGroup("DEBUGGING")][SerializeField] bool EnableAnimationDebugging;

    [HideInInspector] public List<SpawnWave> insertSpawnSequences = new List<SpawnWave>();
    Queue<SpawnRecord> spawnQueue = new Queue<SpawnRecord>();
    
    
    //  #PROPERTIES
    public GamePhase currentGamePhase;                  //  #TODO - Can this be deleted?
    public bool IsAnimationComplete
    {
        get { return gridObjectAnimationInProgress.Count == 0; }
    }

    
    // #FIELDS
    GridManager gridM;
    Player player;    
    Dictionary<GridObject, GridUpdateStep> gridObjectsInPlay;
    List<GridBlock> collisions;
    List<GridObject> gridObjectAnimationInProgress;

    public System.Action<Vector2Int> GridObjectHasDeparted;

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
        gridM = GetComponent<GridManager>();

        hazards     = new List<GridObject>();
        loot        = new List<GridObject>();
        phenomena   = new List<GridObject>();
        stations    = new List<GridObject>();

        gridObjectsInPlay               = new Dictionary<GridObject, GridUpdateStep>();
        gridObjectAnimationInProgress   = new List<GridObject>();
        collisions                      = new List<GridBlock>();
    }
    public void Init(LevelRecord level)
    {
        /*  SUMMARY
         *   - Process LevelTopography cell by cell and instantiate any GridObject found
         *   - Place newly instantiated GridObject into play
         */

        if (level.levelTopography == null)
            Debug.Log("Argument 'topography' is null.");
        else
        {
            int x = level.levelTopography.GetLength(0);
            int y = level.levelTopography.GetLength(1);
            Debug.LogFormat("Argument 'topography' is not NULL.\nWidth: {0}\nHeight: {1}", x, y);
        }
            
        
        for (int x = 0; x < level.levelTopography.GetLength(0); x++)
        {
            for (int y = 0; y < level.levelTopography.GetLength(1); y++)
            {
                TopographyElementIcon element = level.levelTopography[x, y];
                if (element == null)
                {
                    if (VerboseDebugging || EnableInitializationDebugging)
                        Debug.LogFormat("Element at ({0},{1}) is NULL", x, y);
                }
                else
                {
                    if (EnableInitializationDebugging || VerboseDebugging)
                        Debug.LogFormat("Element at ({0},{1}) contains GridObject to spawn", x, y);

                    Vector2Int gridLocation = level.IndexToGrid(x, y);
                    GameObject instance = Instantiate(element.gameObject, gridM.GridToWorld(gridLocation), element.gameObject.transform.rotation);
                    Debug.LogFormat("Instantiated a {0}", instance.name);

                    if (instance == null)
                        Debug.Log("Grrrr, null reference dood");

                    GridObject go = instance.GetComponent<GridObject>();
                    if (go == null)
                        Debug.Log("Something is wrong with GetComponent<>()");

                    if (go is Player)
                        player = go as Player;
                    
                    PlaceGridObjectInPlay(go, gridLocation);
                }                              
            }
        }
    }


    //  #SPAWNING
    public void ApplySpawnWave(SpawnWave received)
    {
        if (received.spawns.Length > 0)
        {
            for (int i = 0; i < received.spawns.Length; i++)
            {
                SpawnRecord current = received.spawns[i];
                GridObject go = Instantiate(current.GridObject);
                PlaceGridObjectInPlay(go, current.GridLocation);

                if (go is Hazard)
                {
                    Hazard newHazard = go as Hazard;
                    newHazard.Init(current.Border);
                }
                else if (go is Loot)
                {
                    Loot newLoot = go as Loot;
                    newLoot.Init(current.Border);
                }
                else
                {
                    go.Init();
                }
            }
        }
        else return;
    }

    void PlaceGridObjectInPlay(GridObject gridObject, Vector2Int gridLocation, bool placeOnGrid = true)
    {
        if (VerboseDebugging) Debug.Log("GridObjectManager.PlaceGridObjectInPlay() called.");

        Vector3 worldLocation = gridM.GridToWorld(gridLocation);

        if (placeOnGrid == false)
        {
            gridObjectsInPlay.Add(gridObject, null);
        }
        else
        {
            if (gridObject == null)
                Debug.Log("gridObject reference is NULL");
            else if (gridObject.gameObject == null)
                Debug.Log("GameObject attached to gridObject is NULL");
                            
            gridObject.gameObject.transform.position = worldLocation;
            gridM.AddObjectToGrid(gridObject.gameObject, gridLocation);
            gridObjectsInPlay.Add(gridObject, null);
        }
    }
    

    //  #CORE FUNCTIONALITY
    public void OnPlayerActivateModule(Thruster.UsageData uData)
    {
        Debug.LogFormat("Thruster.UsageData received. \n{0}", uData.ToString());
        NewGridUpdateSteps();
        LoadGridUpdateSteps(uData);
        RunGridUpdate();
        AnimateMovement();
    }
    public void OnPlayerActivateModule(Weapon.UsageData uData)
    {
        Vector2Int playerGridLocation = gridObjectsInPlay[player].gridOrigin;
        List<GridBlock> possibleTargets = GetGridBlocksInPath(playerGridLocation, player.Direction);
            
        for (int i = 0; i < possibleTargets.Count; i++)
        {
            for (int j = 0; j < possibleTargets[i].objectsOnBlock.Count; j++)
            {
                Health hp = possibleTargets[i].objectsOnBlock[j].GetComponent<Health>();
                if (hp != null)
                {
                    if (uData.DynamicDamage)
                    {
                        int gridBlockDistance = i;
                        int damageAmount = CalculateDamage(gridBlockDistance, uData.BaseDamage, uData.DamageMultiplier);
                        hp.SubtractHealth(damageAmount);
                    }
                    else
                    {
                        hp.SubtractHealth(uData.BaseDamage);
                    }


                    if (!uData.DoesPenetrate)
                    {
                        //  #TO-DO: Execute appropriate animation
                        player.StartWeaponModuleAnimation(possibleTargets[i]);
                        break;
                    }
                }
            }

            if (i == possibleTargets.Count - 1)
                player.StartWeaponModuleAnimation(possibleTargets[i]);
        }
        // End animation at end of targetBlocks List

        if (uData.DoesPlaceObjectInWorld)
        {
            Vector3 playerWorldLocation = gridM.GridToWorld(gridM.FindGridBlockContainingObject(player.gameObject).location);
            GameObject instance = Instantiate(uData.ObjectToPlace, playerWorldLocation, player.transform.rotation);
            instance.GetComponent<GridMover>().SetMovePattern(player.Direction);

            GridObject newObject = instance.GetComponent<GridObject>();
            PlaceGridObjectInPlay(newObject, gridM.WorldToGrid(playerWorldLocation));
        }
    }
    public void OnPlayerActivateModule(Shield.UsageData udata)
    {

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
                else if (gridObjectForStep.TryGetComponent<Thruster>(out Thruster _))
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
    public void LoadGridUpdateSteps(Thruster.UsageData uData = null)
    {
        /*  DESCRIPTION
         *   - Process a list of GridObjects' data
         *   - Determine one of the following behaviors for the current tick for each object:
         *      ~ Depart the grid
         *      ~ Simply move
         *      ~ FlyBy another object
         *      ~ Nothing
         */

        if (VerboseDebugging) Debug.Log("GridObjectManager.LoadGridUpdateSteps called.");

        Vector2Int[] allOriginGridLocations = new Vector2Int[gridObjectsInPlay.Count];          // Fly-By Tracker 1
        Vector2Int[] allDestinationGridLocations = new Vector2Int[gridObjectsInPlay.Count];     // Fly-By Tracker 2

        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            allOriginGridLocations[i].Set(99, 99);
            allDestinationGridLocations[i].Set(99, 99);
        }

        if (VerboseDebugging) Debug.Log("Processing grid object movement.");
        for (int i = 0; i < gridObjectsInPlay.Count; i++)
        {
            KeyValuePair<GridObject, GridUpdateStep> kvp = gridObjectsInPlay.ElementAt(i);
            if (kvp.Value.activeInThisPhase == false)
                continue;

            if (kvp.Value.canMove && uData == null)
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
                        kvp.Value.isDeparting = true;
                }
            }
            else if (kvp.Value.canMove && uData != null)
            {
                if (uData.EligibleToMove)
                {
                    kvp.Value.gridDestination = (kvp.Value.gridOrigin + uData.DirectionToMove) * uData.NumberOfMoves;
                    Debug.LogFormat("Destination set to {0}", kvp.Value.gridDestination.ToString());
                    allOriginGridLocations[i] = kvp.Value.gridOrigin;
                    allDestinationGridLocations[i] = kvp.Value.gridDestination;

                    kvp.Value.isMoving = true;

                    if (!gridM.CheckIfGridBlockInBounds(kvp.Value.gridDestination))
                        kvp.Value.isDeparting = true;
                }
            }
        }

        if (VerboseDebugging) Debug.Log("Fly-By detection starting.");
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

        if (VerboseDebugging) Debug.Log("Begining health check");
    }
    public void RunGridUpdate()
    {
        if (VerboseDebugging) Debug.Log("GridObjectManager.UpdateBoardData() called.");

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
                    if (GridObjectHasDeparted != null)
                        GridObjectHasDeparted(kvp.Value.gridDestination);
                }

                if (kvp.Value.collidesWith != null)
                {
                    GridObject iGridO = kvp.Key;
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
                    gridM.RemoveObjectFromGrid(kvp.Key.gameObject);
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
                StartCoroutine(AnimateMoveCoroutine(kvp.Key, kvp.Value.AnimateDistance, kvp.Value.WorldOrigin, kvp.Value.WorldDestination, kvp.Value.isDeparting));
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
    

    public void ResolveCollisionsOnGridBlocks()
    {
        // i = GridBlock iterator
        // j = objects on GridBlock iterator 1
        // k = objects on GridBlock iterator 2 (j+1)

        for (int i = 0; i < collisions.Count; i++)
        {
            if (collisions[i].objectsOnBlock != null && collisions[i].objectsOnBlock.Count > 1)
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

                                if (VerboseDebugging)
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

                                if (VerboseDebugging)
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
                                    if (VerboseDebugging)
                                        Debug.LogFormat("{0} jump fuel retrieved.", kFuel.fuelAmount);
                                }
                                else
                                {
                                    p.AcceptFuel(kFuel.fuelAmount);

                                    if (VerboseDebugging)
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

                                        if (VerboseDebugging)
                                            Debug.LogFormat("{0} jump fuel retrieved.", jFuel.fuelAmount);
                                    }
                                }
                                else
                                {
                                    p.AcceptFuel(jFuel.fuelAmount);

                                    if (VerboseDebugging)
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
        if (VerboseDebugging)
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

        if (VerboseDebugging)
            Debug.Log("Preparations for next level complete.");
    }
    public void NextLevel()
    {

    }
    public void ArrivePlayer()
    {
        //#TODO
        // Need to add something that will allow for a Player to spawn somewhere other than 0, 0

        Vector2Int arriveLocation = new Vector2Int(0, 0);

        if (VerboseDebugging) Debug.Log("Player Jump successful.  Adding Player to new Grid.");

        if (player.spawnRules.spawnRegion == SpawnRule.SpawnRegion.Center)
        {
            // Can put some new arrival logic here    
        }

        gridM.AddObjectToGrid(player.gameObject, arriveLocation);
        //player.animateStartWorldLocation = gridM.GridToWorld(arriveLocation);
        //player.animateEndWorldLocation = gridM.GridToWorld(arriveLocation);
        player.transform.position = new Vector3(arriveLocation.x, arriveLocation.y, 0);

        if (VerboseDebugging) Debug.Log("Player successfully added to Grid.");
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
    IEnumerator AnimateMoveCoroutine(GridObject objectToMove, float Distance, Vector3 startLocation, Vector3 endLocation, bool destroy=false)
    {
        gridObjectAnimationInProgress.Add(objectToMove);

        float startTime = Time.time;
        float percentTraveled = 0.0f;

        if (EnableAnimationDebugging)
            Debug.LogFormat("Instance of ID of object being move animated: {0}", objectToMove.transform.GetInstanceID().ToString());

        while (percentTraveled <= 1.0)
        {
            if (EnableAnimationDebugging)
                Debug.LogFormat("Beginning to move {0} from {1} to {2}.", objectToMove.gameObject.name, startLocation.ToString(), endLocation.ToString());
            
            float traveled = (Time.time - startTime) * 1.0f;
            percentTraveled = traveled / Distance;
            objectToMove.transform.position = Vector3.Lerp(startLocation, endLocation, Mathf.SmoothStep(0, 1, percentTraveled));

            if (EnableAnimationDebugging)
                Debug.LogFormat("{0} has traveled {1}% of the distance.", objectToMove.gameObject.name, percentTraveled.ToString());
            
            yield return null;
        }

        gridObjectAnimationInProgress.Remove(objectToMove);
        Debug.Log("Move animation complete.");

        if (destroy)
        {
            gridM.RemoveObjectFromGrid(objectToMove.gameObject);
            gridObjectsInPlay.Remove(objectToMove);

            Destroy(objectToMove.gameObject);
        }
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
                lootRotator.ApplyRotation(GridBorder.Left);

                yield return new WaitForSeconds(delayAppear);
                renderer.enabled = true;
            }
        }
    }
}