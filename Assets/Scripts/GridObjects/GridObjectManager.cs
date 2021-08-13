using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridObjectManager : MonoBehaviour
{
    
    [SerializeField] private bool VerboseConsole = true;

    private GridManager gm;
    private Player player;

    private int currentTick = 0;

    private Vector2Int minVector2;
    private Vector2Int maxVector2;

    private List<GridObject> gridObjectsInPlay = new List<GridObject>();            // Object tracking
    

    public enum GamePhase
    {
        Player = 1,
        Manager = 2
    }
    

    
    [Header("Spawn Management")]
    [SerializeField] private int minTicksUntilSpawn = 2;
    [SerializeField] private int maxTicksUntilSpawn = 4;

    [HideInInspector] public List<SpawnSequence> insertSpawnSequences = new List<SpawnSequence>();
    private Queue<SpawnStep> spawnQueue = new Queue<SpawnStep>();
    private int ticksUntilNewSpawn;

    [Header("Grid Object Library")]
    [SerializeField] private GridObject[] gridObjectPrefabs;
    public GameObject playerPrefab;
    
    private List<Phenomena> AllPhenomena
    {
        get
        {
            List<Phenomena> p = new List<Phenomena>();

            for (int i = 0; i < gridObjectPrefabs.Length; i++)
            {
                if (gridObjectPrefabs[i] is Phenomena)
                    p.Add(gridObjectsInPlay[i] as Phenomena);
            }

            return p;
        }
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
        gm = GetComponent<GridManager>();
        
        minVector2 = new Vector2Int(gm.BoundaryLeftActual, gm.BoundaryBottomActual);
        maxVector2 = new Vector2Int(gm.BoundaryRightActual, gm.BoundaryTopActual);

        ticksUntilNewSpawn = Random.Range(minTicksUntilSpawn, maxTicksUntilSpawn);
        currentTick = 1;

        gridObjectsInPlay.Insert(0, GameObject.FindWithTag("Player").GetComponent<Player>());
        player = gridObjectsInPlay[0].GetComponent<Player>();

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
            AddSpawnStep(SelectGridObject(GridObjectType.Hazard));
            CreateGridObject(spawnQueue.Dequeue());
        }
    }

    private GridObject SelectGridObject(GridObjectType type)
    {
        if(type == GridObjectType.Hazard)
        {
            List<Hazard> availableHazards = new List<Hazard>();
            for (int i = 0; i < gridObjectPrefabs.Length; i++)
            {
                if (gridObjectPrefabs[i] is Hazard)
                    availableHazards.Add(gridObjectPrefabs[i] as Hazard);
            }

            int selector = Random.Range(0, availableHazards.Count - 1);
            if (VerboseConsole) Debug.Log("Selected a Hazard.");
            return availableHazards[selector];
        }
        else if(type == GridObjectType.Phenomena)
        {
            List<Phenomena> availablePhenomena = new List<Phenomena>();
            for (int i = 0; i < gridObjectPrefabs.Length; i++)
            {
                if (gridObjectPrefabs[i] is Phenomena)
                    availablePhenomena.Add(gridObjectPrefabs[i] as Phenomena);
            }

            int selector = Random.Range(0, availablePhenomena.Count - 1);
            if (VerboseConsole) Debug.Log("Selected a Phenomenon.");
            return availablePhenomena[selector];
        }
        else
        {
            return null;
        }


    }


    private void AddSpawnStep(GridObject objectForSpawn)
    {
        /*	SUMMARY
         *  -   Randomly selects a spawn location
         *  -   Instantiates SpawnStep ScriptableObject
         *  -   Initializes the new SpawnStep
         *  -   Enqueue the new SpawnStep
         */

        if (VerboseConsole) Debug.Log("GridObjectManager.CreateSpawnStep() called.");

        //int gridObjectSelector = Random.Range(0, gridObjectPrefabs.Length);

        Vector2Int hazardSpawnLocation = new Vector2Int();

        // DEBUG: Make sure spawn selection is working appropriately
        //if (VerboseConsole) Debug.LogFormat("Array Length: {0}, Random value: {1}", gridObjectPrefabs.Length, gridObjectSelector);

        // Identify an appropriate spawn location
//        List<Vector2Int> availableSpawns = gm.GetSpawnLocations(gridObjectPrefabs[gridObjectSelector].spawnRules);
        List<Vector2Int> availableSpawns = gm.GetSpawnLocations(objectForSpawn.spawnRules);

        Vector2Int targetLocation = availableSpawns[Random.Range(0, availableSpawns.Count)];
        hazardSpawnLocation.Set(targetLocation.x, targetLocation.y);

        // Create the SpawnStep
        SpawnStep newSpawnStep = ScriptableObject.CreateInstance<SpawnStep>();
//        newSpawnStep.Init(gridObjectPrefabs[gridObjectSelector], hazardSpawnLocation);
        newSpawnStep.Init(objectForSpawn, hazardSpawnLocation);
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

        GridObject newSpawn = Instantiate(spawnStep.gridObject);

        string borderName = "";
        if (spawnStep.SpawnLocation.y == gm.BoundaryBottomActual) borderName = "Bottom";
        else if (spawnStep.SpawnLocation.y == gm.BoundaryTopActual) borderName = "Top";
        else if (spawnStep.SpawnLocation.x == gm.BoundaryRightActual) borderName = "Right";
        else if (spawnStep.SpawnLocation.x == gm.BoundaryLeftActual) borderName = "Left";

        newSpawn.Init(borderName);

        //Debug line
        //Hazard hazardToSpawn = Instantiate(hazardPrefabs[1]);

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
    public void ArrivePlayer()
    {
        //#TODO
        // Need to add something that will allow for a Player to spawn somewhere other than 0, 0

        Vector2Int arriveLocation = new Vector2Int(0, 0);

        if (VerboseConsole) Debug.Log("Player Jump successful.  Adding Player to new Grid.");

        if(player.spawnRules.spawnRegion == GridManager.SpawnRule.SpawnRegion.Center)
        {
            // Can put some new arrival logic here    
        }

        gm.AddObjectToGrid(player.gameObject, arriveLocation);
        player.currentWorldLocation = gm.GridToWorld(arriveLocation);
        player.targetWorldLocation = gm.GridToWorld(arriveLocation);
        player.transform.position = new Vector3(arriveLocation.x, arriveLocation.y, 0);

        if (VerboseConsole) Debug.Log("Player successfully added to Grid.");
    }


    private List<GridBlock> MoveGridObjectsForTick(List<GridObject> objects)
    {
        /*  PLAN
         *   - Process a list of GridObjects' data
         *   - Determine one of the following behaviors for the current tick for each object:
         *      ~ Depart the grid
         *      ~ Simply move
         *      ~ FlyBy another object
         *      ~ Nothing
         */

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
            //MovePattern mp = objects[i].GetComponent<MovePattern>();    //IMovable
            if(objects[i].TryGetComponent<MovePattern>(out MovePattern mp))
            {
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
                Vector2Int currentLocation = gm.WorldToGrid(objects[i].currentWorldLocation);
                Vector2Int destinationLocation = currentLocation + mp.DirectionOnGrid;

                if (mp.CanMoveThisTurn)
                {
                    if (objects[i].IsLeavingGrid == false && objects[i].GetComponent<Health>().HasHP)
                    {
                        // Eligible to move
                        gm.RemoveObjectFromGrid(objects[i].gameObject, currentLocation);
                        gm.AddObjectToGrid(objects[i].gameObject, destinationLocation);

                        objects[i].targetWorldLocation = gm.GridToWorld(destinationLocation);

                        if (!IsLocationInCollisionTracker(destinationLocation, collisionTestBlocks))
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
            //MovePattern mp = objects[i].GetComponent<MovePattern>();
            //mp.OnTickUpdate();

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

                    if (playerWeapon.RequiresGridPlacement)
                    {
                        //GameObject weaponInstance = Instantiate(player.SelectedWeaponProjectile, player.currentWorldLocation, player.transform.rotation);
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
                                    hp.SubtractHealth(player.SelectedWeapon.Damage);

                                    if (!player.SelectedWeapon.DoesPenetrate)
                                    {
                                        player.ExecuteAttackAnimation(targetBlocks[i]);
                                        // force i to = targetBlocks.Count? #Question
                                        break;
                                    }
                                }
                            }
                        }
                        // End animation at end of targetBlocks List
                        player.ExecuteAttackAnimation(targetBlocks[targetBlocks.Count - 1]);
                    }

                    playerWeapon.SubtractAmmo();
                    StartCoroutine(player.UpdateUICoroutine());
                }
            }

            potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);
            //ProcessCollisionsOnGridBlock(potentialBlockCollisions);
            player.IsAttackingThisTick = false;
        }

        if (phase == GamePhase.Manager)
        {
            CheckHealth(gridObjectsInPlay);

            // Process GridObject behavior for current Tick
            for (int i = 0; i < gridObjectsInPlay.Count; i++)
            {
                if (gridObjectsInPlay[i].ProcessingPhase == GamePhase.Manager)
                {
                    Health hp = gridObjectsInPlay[i].GetComponent<Health>();

                    if (hp != null & hp.HasHP)
                        objectProcessing.Add(gridObjectsInPlay[i]);
                }
                            
            }
            potentialBlockCollisions = MoveGridObjectsForTick(objectProcessing);

            gridObjectDestroyedThisTick = CheckHealth(gridObjectsInPlay, 1.0f);

            // Phenomena Processing


            // Spawn stuff
            if (ticksUntilNewSpawn == 0 || gridObjectsInPlay.Count == 0)
            {
                if (spawnQueue.Count > 0)
                {
                    CreateGridObject(spawnQueue.Dequeue());
                }
                else
                {
                    AddSpawnStep(SelectGridObject(GridObjectType.Hazard));
                    CreateGridObject(spawnQueue.Dequeue());
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

        //StartCoroutine(AnimateTick);
    }
    public void ClearLevel()
    {
        if (VerboseConsole)
            Debug.Log("Preparing for next level. Removing all GridObjects in play.");

        for (int i = gridObjectsInPlay.Count - 1; i > 0; i--)
        {
            //RemoveGridObjectFromPlay(gridObjectsInPlay[i]);
            Destroy(gridObjectsInPlay[i].gameObject);
            gridObjectsInPlay.RemoveAt(i);
        }

        spawnQueue.Clear();
        //gridObjectsInPlay[0].gameObject.SetActive(false);       //Disable Player 
        //StartCoroutine(player.AnimateNextLevel());

        if (VerboseConsole)
            Debug.Log("Preparations for next level complete.");
    }

    public void NextLevel(int phenomenaRequired)
    {
        // For each phenomenaRequired, should randomly select one 
        for (int i = 0; i < phenomenaRequired; i++)
        {
            AddSpawnStep(SelectGridObject(GridObjectType.Phenomena));
            CreateGridObject(spawnQueue.Dequeue());
        }
        // Create SpawnSteps

    }  


    private void ProcessCollisionsOnGridBlock(List<GridBlock> gridBlocks)
    {
        // i = GridBlock iterator
        // j = objects on GridBlock iterator 1
        // k = objects on GridBlock iterator 2 (j+1)

        for (int i = 0; i < gridBlocks.Count; i++)
        {
            if (gridBlocks[i].objectsOnBlock.Count > 1)
            {
                Debug.Log("Processing collision on " + gridBlocks[i].location.ToString());

                //  Consider this
                //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/local-functions
                // Also can use gridObject.ObjectType property for processing Hazards & Phenomena

                for (int j = 0; j < gridBlocks[i].objectsOnBlock.Count; j++)
                {
                    GridObject jGridObject = gridBlocks[i].objectsOnBlock[j].GetComponent<GridObject>();

                    //GameObject gameObject = gridBlocks[i].objectsOnBlock[j];
                    //GridObject gridObject = gameObject.GetComponent<GridObject>();
                    //Loot loot = gameObject.GetComponent<Loot>();
                    //Health gameObjectHealth = gameObject.GetComponent<Health>();
                    //ContactDamage gameObjectDamage = gameObject.GetComponent<ContactDamage>();
                    //Phenomena phenomena = gameObject.GetComponent<Phenomena>();

                    for (int k = 1 + j; k < gridBlocks[i].objectsOnBlock.Count; k++)
                    {
                        GridObject kGridObject = gridBlocks[i].objectsOnBlock[k].GetComponent<GridObject>();

                        bool jIsPlayer = jGridObject is Player;
                        bool kIsPlayer = kGridObject is Player;

                        bool jIsPhenomena = jGridObject is Phenomena;
                        bool kIsPhenomena = kGridObject is Phenomena;

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

                        if (jHasHP && kDoesDamage)
                        {
                            if (!jIsPlayer && !kIsPhenomena && !kDoesSupply)     //Isn't a Supply Station
                                jHealth.SubtractHealth(kDamage.DamageAmount);
                            else if (!jIsPlayer && !kIsPhenomena && !kDoesFuel)  //Isn't a Fuel Station
                                jHealth.SubtractHealth(kDamage.DamageAmount);
                        }

                        if (kHasHP && jDoesDamage)
                        {
                            if (!kIsPlayer && !jIsPhenomena && !jDoesSupply)     //Is a Supply Station
                                kHealth.SubtractHealth(jDamage.DamageAmount);
                            else if (!kIsPlayer && !jIsPhenomena && !jDoesFuel)  //Is a Fuel Station
                                kHealth.SubtractHealth(jDamage.DamageAmount);
                        }   

                        if ( (jIsPlayer && kDoesSupply) || (kIsPlayer && jDoesSupply) )
                        {
                            if (jIsPlayer)
                            {
                                Player p = jGridObject as Player;
                                ContactSupply cs = kGridObject.GetComponent<ContactSupply>();

                                if (VerboseConsole)
                                    Debug.LogFormat("Player is picking up {0} of {1} ammo.", cs.supplyAmount.ToString(), cs.weaponType.ToString());

                                p.AcceptAmmo(cs.weaponType, cs.supplyAmount);
                                if (kIsLoot)
                                    kHealth.SubtractHealth(kHealth.CurrentHP);
                            }
                            else if (kIsPlayer)
                            {
                                Player p = kGridObject as Player;
                                ContactSupply cs = jGridObject.GetComponent<ContactSupply>();

                                if (VerboseConsole)
                                    Debug.LogFormat("Player is picking up {0} of {1} ammo.", cs.supplyAmount.ToString(), cs.weaponType.ToString());

                                p.AcceptAmmo(cs.weaponType, cs.supplyAmount);
                                if (jIsLoot)
                                    jHealth.SubtractHealth(jHealth.CurrentHP);
                            }
                        }

                        if ( (jIsPlayer && kDoesFuel) || (kIsPlayer && jDoesFuel))
                        {
                            if (jIsPlayer)
                            {
                                Player p = jGridObject as Player;
                                p.AcceptFuel(kFuel.fuelAmount);
                                p.UpdateUICoroutine();

                                if (VerboseConsole)
                                    Debug.LogFormat("{0} jump fuel retrieved.", kFuel.fuelAmount);
                            }
                            else if (kIsPlayer)
                            {
                                Player p = kGridObject as Player;
                                p.AcceptFuel(jFuel.fuelAmount);
                                p.UpdateUICoroutine();

                                if (VerboseConsole)
                                    Debug.LogFormat("{0} jump fuel retrieved.", jFuel.fuelAmount);
                            }
                        }

                        if ( (jIsPlayer && kDoesRepair) || (kIsPlayer && jDoesRepair) )
                        {
                            if (jIsPlayer)
                            {
                                jHealth.AddHealth(kRepair.repairAmount);
                                Player p = jGridObject as Player;
                                p.UpdateUICoroutine();
                            }
                            else if (kIsPlayer)
                            {
                                kHealth.AddHealth(jRepair.repairAmount);
                                Player p = kGridObject as Player;
                                p.UpdateUICoroutine();
                            }
                                
                        }

                        //GameObject otherGameObject = gridBlocks[i].objectsOnBlock[k];
                        //GridObject otherGridObject = otherGameObject.GetComponent<GridObject>();
                        //Loot otherLoot = otherGameObject.GetComponent<Loot>();
                        //Health otherGameObjectHealth = otherGameObject.GetComponent<Health>();
                        //ContactDamage otherGameObjectDamage = otherGameObject.GetComponent<ContactDamage>();
                        //Phenomena otherPhenomena = otherGameObject.GetComponent<Phenomena>();

                        
/*
                        if (gameObjectDamage != null && otherGameObjectHealth != null)
                        {
                            if (VerboseConsole)
                                Debug.Log("Subtracting " + gameObjectDamage.DamageAmount.ToString() + " from " + otherGameObject.name);

                            otherGameObjectHealth.SubtractHealth(gameObjectDamage.DamageAmount);
                        }

                        if (gameObjectHealth != null && otherGameObjectDamage != null)
                        {
                            if (VerboseConsole)
                                Debug.Log("Subtracting " + otherGameObjectDamage.DamageAmount.ToString() + " from " + gameObject.name);

                            gameObjectHealth.SubtractHealth(otherGameObjectDamage.DamageAmount);
                        }
*/

                    }
                }
            }
        }
    }
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

    private bool CheckHealth(List<GridObject> objects, float delayAnimation = 0.0f)
    {
        bool returnBool = false;
        for (int i = objects.Count - 1; i > -1; i--)
        {
            Health hp = objects[i].GetComponent<Health>();
            if (hp != null && !hp.HasHP)
            {
                StartCoroutine(DropLootCoroutine(objects[i], objects[i].targetWorldLocation, delayAnimation));    // <- objects[i].currentWorldLocation
                StartCoroutine(GridObjectDestructionCoroutine(objects[i], delayAnimation));
                
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

            if (lootObjectToDrop != null)
            {
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

public enum GridObjectType
{
    Hazard = 1,
    Phenomena = 2
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