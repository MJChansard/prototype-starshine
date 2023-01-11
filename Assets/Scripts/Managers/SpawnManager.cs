using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    //  # INSPECTOR
    [TitleGroup("CURRENT STATUS")][ShowInInspector][DisplayAsString] Dictionary<GridBorder, List<Vector2Int>> AvailableSpawns;
    [BoxGroup("GENERAL COMPONENT CONFIGURATION", centerLabel: true)]
    [SerializeField] bool VerboseConsole;
    [BoxGroup("GENERAL COMPONENT CONFIGURATION")][SerializeField] bool forceSpawnEveryTurn;

    [BoxGroup("CUSTOM SPAWN CONFIGURATION", centerLabel: true)]
    [BoxGroup("CUSTOM SPAWN CONFIGURATION")][SerializeField] bool enable;
    [ShowIf("enable")]
    [BoxGroup("CUSTOM SPAWN CONFIGURATION")][SerializeField] List<SpawnWave> customSpawnSequence;

    /*  IDEA
     *  - Would be great to leverage Odin here
     *  - Have buttons that can manage the creation and insertion of custom SpawnSteps
     *  - Would be so much better than a library of ScriptableObjects that I have to drag and drop into sequences
     */

    //  # PROPERTIES
    public bool CustomSpawnSequenceExist { get { return customSpawnSequence.Count == 0; } }
    public SpawnWave GetSpawnWave
    {
        get
        {
            if (spawnQueue.Count == 0)
                EnqueueNewSpawnWave(1);

            SpawnWave result = spawnQueue.Dequeue();
            EnqueueNewSpawnWave(1);

            return result;
        }
    }
    public bool ForceSpawnEveryTurn { get { return forceSpawnEveryTurn; } }

    List<Vector2Int> remainingInteriorSpawns
    {
        get
        {
            List<Vector2Int> result = new List<Vector2Int>();
            foreach (var spawn in eligibleInteriorSpawns)
            {
                if (!takenSpawns.Contains(spawn))
                    result.Add(spawn);
            }
            return result;
        }
    }
    List<Vector2Int> remainingTopSpawns
    {
        get
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int test = new Vector2Int();

            for (int x = thisLevel.BoundaryLeftActual + 1; x < thisLevel.BoundaryRightActual; x++)  //Offset left by 1, don't want corner
            {
                test.Set(x, thisLevel.BoundaryTopActual);
                if (!takenSpawns.Contains(test))
                    result.Add(test);
            }
            return result;
        }
    }
    List<Vector2Int> remainingBottomSpawns
    {
        get
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int test = new Vector2Int();

            for (int x = thisLevel.BoundaryLeftActual + 1; x < thisLevel.BoundaryRightActual; x++)  //Offset left by 1, don't want corner
            {
                test.Set(x, thisLevel.BoundaryBottomActual);
                if (!takenSpawns.Contains(test))
                    result.Add(test);
            }
            return result;
        }
    }
    List<Vector2Int> remainingLeftSpawns
    {
        get
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int test = new Vector2Int();

            for (int y = thisLevel.BoundaryBottomActual + 1; y < thisLevel.BoundaryTopActual; y++)  //Offset bottom by 1, don't want corner
            {
                test.Set(thisLevel.BoundaryLeftActual, y);
                if (!takenSpawns.Contains(test))
                    result.Add(test);
            }
            return result;
        }
    }
    List<Vector2Int> remainingRightSpawns
    {
        get
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int test = new Vector2Int();

            for (int y = thisLevel.BoundaryBottomActual + 1; y < thisLevel.BoundaryTopActual; y++)  //Offset bottom by 1, don't want corner
            {
                test.Set(thisLevel.BoundaryRightActual, y);
                if (!takenSpawns.Contains(test))
                    result.Add(test);
            }
            return result;
        }
    }
    int CountAvailableBorderSpawns
    {
        get
        {
            return availableBorderSpawns[GridBorder.Top].Count +
                availableBorderSpawns[GridBorder.Bottom].Count +
                availableBorderSpawns[GridBorder.Left].Count +
                availableBorderSpawns[GridBorder.Right].Count;
        }
    }




    //  # FIELDS
    List<GridObject> hazards = new();
    List<GridObject> loot = new();
    List<GridObject> phenomena;
    List<GridObject> stations;

    //List<Vector2Int> eligiblePerimeterSpawns;
    List<Vector2Int> eligibleInteriorSpawns = new();
    List<Vector2Int> takenSpawns = new();
    Queue<SpawnWave> spawnQueue = new();

    Dictionary<GridBorder, List<Vector2Int>> availableBorderSpawns = new();

    LevelRecord thisLevel;


    private void Awake()
    {
        availableBorderSpawns.Add(GridBorder.Top, new List<Vector2Int>());
        availableBorderSpawns.Add(GridBorder.Bottom, new List<Vector2Int>());
        availableBorderSpawns.Add(GridBorder.Left, new List<Vector2Int>());
        availableBorderSpawns.Add(GridBorder.Right, new List<Vector2Int>());

        customSpawnSequence = new List<SpawnWave>();

        AvailableSpawns = availableBorderSpawns;
    }

    public void Init(LevelRecord level)
    {
        thisLevel = level;
        for (int i = 0; i < level.spawnableGridObjects.Length; i++)
        {
            GridObject go = level.spawnableGridObjects[i];
            if (go is Hazard)
                hazards.Add(go as Hazard);
            else if (go is Loot)
                loot.Add(go as Loot);
            else if (go is Phenomena)
                phenomena.Add(go as Phenomena);
            else if (go is Station)
                stations.Add(go as Station);
        }

        FindEligibleSpawns();

        if (customSpawnSequence.Count > 0)
        {
            for (int i = 0; i < customSpawnSequence.Count; i++)
            {
                spawnQueue.Enqueue(customSpawnSequence[i]);
            }
        }
        else
        {
            EnqueueNewSpawnWave(3);
        }
    }

    void FindEligibleSpawns()
    {
        for (int x = thisLevel.BoundaryLeftActual; x <= thisLevel.BoundaryRightActual; x++)
        {
            for (int y = thisLevel.BoundaryBottomActual; y < thisLevel.BoundaryTopActual; y++)
            {
                Vector2Int gridLocation = new Vector2Int(x, y);

                if (x == thisLevel.BoundaryLeftActual && y != thisLevel.BoundaryTopActual && y != thisLevel.BoundaryBottomActual)
                {
                    availableBorderSpawns[GridBorder.Left].Add(gridLocation);
                }
                else if (x == thisLevel.BoundaryRightActual && y != thisLevel.BoundaryTopActual && y != thisLevel.BoundaryBottomActual)
                {
                    availableBorderSpawns[GridBorder.Right].Add(gridLocation);
                }
                else if (x != thisLevel.BoundaryLeftActual && x != thisLevel.BoundaryRightActual && y == thisLevel.BoundaryTopActual)
                {
                    availableBorderSpawns[GridBorder.Top].Add(gridLocation);
                }
                else if (x != thisLevel.BoundaryLeftActual && x != thisLevel.BoundaryRightActual && y == thisLevel.BoundaryBottomActual)
                {
                    availableBorderSpawns[GridBorder.Bottom].Add(gridLocation);
                }
                else if ((x > thisLevel.BoundaryLeftActual && x < thisLevel.BoundaryRightActual && y < thisLevel.BoundaryTopActual && y > thisLevel.BoundaryBottomActual))
                {
                    eligibleInteriorSpawns.Add(gridLocation);
                }
            }
        }
    }

    void EnqueueNewSpawnWave(int waveCount)
    {
        int numberOfObjectsInWave = 0;
        //Dictionary<GridBorder, int> spawnBorderTracker = new Dictionary<GridBorder, int>();

        for (int w = 0; w < waveCount; w++)
        {
            int maxObjectsForWave = CountAvailableBorderSpawns >= thisLevel.maxObjectsPerWave ? thisLevel.maxObjectsPerWave : CountAvailableBorderSpawns;
            numberOfObjectsInWave = Random.Range(thisLevel.minObjectsPerWave, maxObjectsForWave + 1);
            SpawnWave newWave = SpawnWave.CreateSpawnWave(numberOfObjectsInWave);

            for (int i = 0; i < numberOfObjectsInWave; i++)
            {
                SpawnRecord sr = SpawnRecord.CreateSpawnRecord();

                if (loot.Count > 0)
                {
                    if (Random.Range(1, 11) > 2)
                        sr.GridObject = hazards[Random.Range(0, hazards.Count)];
                    else
                        sr.GridObject = loot[Random.Range(0, loot.Count)];
                }
                else
                {
                    sr.GridObject = hazards[Random.Range(0, hazards.Count)];
                }


                if (sr.GridObject.spawnRules.spawnRegion == SpawnRule.SpawnRegion.Interior)
                {
                    // Start with eligibleInteriorSpawns
                }
                else if (sr.GridObject.spawnRules.spawnRegion == SpawnRule.SpawnRegion.Perimeter)
                {
                    int x = Random.Range(0, thisLevel.bordersEligibleForSpawn.Length);
                    GridBorder selected = thisLevel.bordersEligibleForSpawn[x];
                    
                    int randomizerMax = availableBorderSpawns[selected].Count;
                    sr.GridLocation = availableBorderSpawns[selected][Random.Range(0, randomizerMax)];
                    sr.Border = selected;
                    availableBorderSpawns[selected].Remove(sr.GridLocation);

                    if (sr.GridObject.spawnRules.avoidHazardPaths)
                    {
                        Vector2Int? oppositeGridBlock = thisLevel.GetOppositeBorderGridBlock(sr.GridLocation);
                        if (oppositeGridBlock.HasValue)
                        {
                            Vector2Int target = (Vector2Int)oppositeGridBlock;
                            GridBorder oppositeBorder = thisLevel.GetGridBorderOfGridBlock(target);
                            availableBorderSpawns[oppositeBorder].Remove(target);
                        }
                    }

                    newWave.spawns[i] = sr;
                }
            }
            spawnQueue.Enqueue(newWave);
        }
    }

    SpawnWave CreateSpawnsForLevel(int phenomenaCount, int stationCount)
    {
        /*  NOTE
            * 
            *  Not currently used but keeping here in case I can re-use the interior spawning logic
            */
        SpawnWave level = SpawnWave.CreateSpawnWave(phenomenaCount + stationCount);

        if (phenomenaCount > 0 || stationCount > 0)
            level.spawns = new SpawnRecord[phenomenaCount + stationCount];
        else
            return level;

        int arrayIndex = 0;
        if (phenomenaCount > 0)
        {
            for (int i = 0; i < phenomenaCount; i++)
            {
                SpawnRecord current = SpawnRecord.CreateSpawnRecord();
                current.GridObject = phenomena[Random.Range(0, phenomena.Count)];
                Vector2Int gridLocation = remainingInteriorSpawns[Random.Range(0, remainingInteriorSpawns.Count)];
                takenSpawns.Add(gridLocation);
                level.spawns[i] = current;
                arrayIndex++;
            }
        }

        if (stationCount > 0)
        {
            for (int i = arrayIndex; i < level.spawns.Length; i++)
            {
                SpawnRecord current = SpawnRecord.CreateSpawnRecord();
                current.GridObject = stations[Random.Range(0, stations.Count)];
                Vector2Int gridLocation = remainingInteriorSpawns[(Random.Range(0, stations.Count))];
                takenSpawns.Add(gridLocation);
                level.spawns[i] = current;
            }
        }

        return level;
    }
}
    //void AddSpawnStep(GridObject objectForSpawn)
    //{
    //    /*	SUMMARY
    //     *  -   Randomly selects a spawn location
    //     *  -   Instantiates SpawnStep ScriptableObject
    //     *  -   Initializes the new SpawnStep
    //     *  -   Enqueue the new SpawnStep
    //     */

    //    if (VerboseConsole) Debug.Log("GridObjectManager.CreateSpawnStep() called.");

    //    Vector2Int hazardSpawnLocation = new Vector2Int();

    //    // DEBUG: Make sure spawn selection is working appropriately
    //    //if (VerboseConsole) Debug.LogFormat("Array Length: {0}, Random value: {1}", gridObjectPrefabs.Length, gridObjectSelector);

    //    // Identify an appropriate spawn location
    //    //        List<Vector2Int> availableSpawns = gm.GetSpawnLocations(gridObjectPrefabs[gridObjectSelector].spawnRules);

    //    List<Vector2Int> allAvailableSpawns = gridM.GetSpawnLocations(objectForSpawn.spawnRules.spawnRegion);
    //    List<Vector2Int> finalAvailableSpawns = ResolveSpawns(allAvailableSpawns, objectForSpawn.spawnRules);

    //    Vector2Int targetLocation = finalAvailableSpawns[Random.Range(0, allAvailableSpawns.Count)];
    //    hazardSpawnLocation.Set(targetLocation.x, targetLocation.y);

    //    // Create the SpawnStep
    //    SpawnStep newSpawnStep = ScriptableObject.CreateInstance<SpawnStep>();
    //    //        newSpawnStep.Init(gridObjectPrefabs[gridObjectSelector], hazardSpawnLocation);
    //    newSpawnStep.Init(objectForSpawn, hazardSpawnLocation);
    //    spawnQueue.Enqueue(newSpawnStep);
    //}
    //GridObject SelectGridObject(GridObjectType type)
    //{
    //    if (type == GridObjectType.Hazard)
    //    {

    //        int selector = Random.Range(0, hazards.Count - 1);
    //        if (VerboseConsole) Debug.Log("Selected a Hazard.");
    //        return hazards[selector];
    //    }
    //    else if (type == GridObjectType.Station)
    //    {
    //        int selector = Random.Range(0, stations.Count - 1);
    //        if (VerboseConsole) Debug.Log("Selected a Station.");
    //        return stations[selector];
    //    }
    //    else if (type == GridObjectType.Phenomena)
    //    {
    //        int selector = Random.Range(0, phenomena.Count);
    //        if (VerboseConsole) Debug.Log("Selected a Phenomenon.");
    //        return phenomena[selector];
    //    }
    //    else if (type == GridObjectType.Loot)
    //    {
    //        int selector = Random.Range(0, loot.Count);
    //        if (VerboseConsole) Debug.Log("Selected a Loot.");
    //        return loot[selector];
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}
    //void CreateGridObject(SpawnStep spawnStep)
    //{
    //    /*  SUMMARY
    //    *   - Process SpawnStep data to identify hazard to spawn and spawn location
    //    *   - Instantiate hazard
    //    *   - Prepare hazard for gameplay
    //    *       ~ Set Animation Mode
    //    *       ~ Toggle Invincibility
    //    *       ~ Activates Rotator
    //    *       ~ Sets MovePattern
    //    */

    //    if (VerboseConsole) Debug.Log("GridObjectManager.CreateHazard() called.");

    //    GridObject newSpawn = Instantiate(spawnStep.gridObject);

    //    if (newSpawn.spawnRules.spawnRegion == GridManager.SpawnRule.SpawnRegion.Perimeter)
    //    {
    //        string borderName = "";
    //        if (spawnStep.SpawnLocation.y == gridM.BoundaryBottomActual) borderName = "Bottom";
    //        else if (spawnStep.SpawnLocation.y == gridM.BoundaryTopActual) borderName = "Top";
    //        else if (spawnStep.SpawnLocation.x == gridM.BoundaryRightActual) borderName = "Right";
    //        else if (spawnStep.SpawnLocation.x == gridM.BoundaryLeftActual) borderName = "Left";

    //        //newSpawn.Init(borderName);
    //        if (newSpawn is Hazard)
    //        {
    //            Hazard newHazard = newSpawn as Hazard;
    //            newHazard.spawnBorder = borderName;
    //            newHazard.Init();
    //        }
    //        else if (newSpawn is Loot)
    //        {
    //            Loot newLoot = newSpawn as Loot;
    //            newLoot.spawnBorder = borderName;
    //            newLoot.Init();
    //        }
    //        else
    //        {
    //            newSpawn.Init();
    //        }

    //    }

    //    PlaceGridObjectInPlay(newSpawn, spawnStep.SpawnLocation);

    //    if (VerboseConsole) Debug.Log("GridObjectManager.CreateGridObject() completed.");
    //}
    //public void PlaceGridObjectInPlay(GridObject gridObject, Vector2Int gridLocation, bool placeOnGrid = true)
    //{
    //    if (VerboseConsole) Debug.Log("GridObjectManager.PlaceGridObjectInPlay() called.");

    //    Vector3 worldLocation = gridM.GridToWorld(gridLocation);

    //    if (placeOnGrid == false)
    //    {
    //        gridObjectsInPlay.Add(gridObject, null);
    //    }
    //    else
    //    {
    //        gridObject.transform.position = worldLocation;
    //        gridM.AddObjectToGrid(gridObject.gameObject, gridLocation);
    //        gridObjectsInPlay.Add(gridObject, null);
    //    }
    //}
    //List<Vector2Int> ResolveSpawns(List<Vector2Int> possibleSpawns, GridManager.SpawnRule rule)
    //{
    //    List<Vector2Int> ineligibleSpawns = new List<Vector2Int>();
    //    if (rule.avoidHazardPaths)
    //    {
    //        for (int i = 0; i < gridObjectsInPlay.Count; i++)
    //        {

    //            if (gridObjectsInPlay.Keys.ElementAt(i).TryGetComponent<Hazard>(out Hazard h) && gridObjectsInPlay.Keys.ElementAt(i).TryGetComponent<GridMover>(out GridMover mp))
    //            {
    //                Vector2Int gridLocation = gridM.WorldToGrid(h.animateStartWorldLocation);
    //                Vector2Int direction = mp.DirectionOnGrid;

    //                bool onLeftBoundary = false;
    //                bool onRightBoundary = false;
    //                bool onTopBoundary = false;
    //                bool onBottomBoundary = false;

    //                if (gridLocation.x == gridM.BoundaryLeftPlay)
    //                    onLeftBoundary = true;
    //                else if (gridLocation.x == gridM.BoundaryRightPlay)
    //                    onRightBoundary = true;
    //                else if (gridLocation.y == gridM.BoundaryTopPlay)
    //                    onTopBoundary = true;
    //                else if (gridLocation.y == gridM.BoundaryBottomPlay)
    //                    onBottomBoundary = true;

    //                if (direction == Vector2Int.up)
    //                {
    //                    // Disable spawning on opposing GridBlock at boundary
    //                    Vector2Int oppositeBoundary = new Vector2Int(gridLocation.x, gridM.BoundaryTopActual);
    //                    ineligibleSpawns.Add(oppositeBoundary);

    //                    // Remove neighboring GridBlocks along boundary
    //                    if (onLeftBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y + 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }

    //                    if (onRightBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x + 1, gridLocation.y + 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }
    //                }

    //                if (direction == Vector2Int.down)
    //                {
    //                    // Disable spawning on opposing GridBlock at boundary
    //                    Vector2Int oppositeBoundary = new Vector2Int(gridLocation.x, gridM.BoundaryBottomActual);
    //                    ineligibleSpawns.Add(oppositeBoundary);

    //                    // Remove neighboring GridBlocks along boundary
    //                    if (onLeftBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y - 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }

    //                    if (onRightBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x + 1, gridLocation.y - 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }
    //                }

    //                if (direction == Vector2Int.left)
    //                {
    //                    // Disable spawning on opposing GridBlock at boundary
    //                    Vector2Int oppositeBoundary = new Vector2Int(gridM.BoundaryLeftActual, gridLocation.y);
    //                    ineligibleSpawns.Add(oppositeBoundary);

    //                    // Remove neighboring GridBlocks along boundary
    //                    if (onTopBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y + 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }

    //                    if (onBottomBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y - 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }
    //                }

    //                if (direction == Vector2Int.right)
    //                {
    //                    // Disable spawning on opposing GridBlock at boundary
    //                    Vector2Int oppositeBoundary = new Vector2Int(gridM.BoundaryRightActual, gridLocation.y);
    //                    ineligibleSpawns.Add(oppositeBoundary);

    //                    // Remove neighboring GridBlocks along boundary
    //                    if (onLeftBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x - 1, gridLocation.y + 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }

    //                    if (onRightBoundary)
    //                    {
    //                        Vector2Int neighbor = new Vector2Int(gridLocation.x + 1, gridLocation.y + 1);
    //                        ineligibleSpawns.Add(neighbor);
    //                    }
    //                }
    //            }
    //        }

    //    }

    //    if (rule.avoidAdjacentToPlayer)
    //    {
    //        // Remove Player location and surrounding GridBlocks
    //        Vector2Int pLocation = gridM.WorldToGrid(player.animateStartWorldLocation);
    //        for (int x = pLocation.x - 1; x < pLocation.x + 2; x++)
    //        {
    //            for (int y = pLocation.y + 1; y > (pLocation.y - 2); y--)
    //            {
    //                Vector2Int area = new Vector2Int(x, y);
    //                ineligibleSpawns.Add(area);
    //            }
    //        }
    //    }

    //    if (rule.avoidShareSpawnLocation)
    //    {
    //        for (int i = 0; i < gridObjectsInPlay.Count; i++)
    //        {
    //            ineligibleSpawns.Add(gridM.WorldToGrid(gridObjectsInPlay.Keys.ElementAt(i).animateStartWorldLocation));
    //        }
    //    }

    //    //List<Vector2Int> resolvedSpawns = possibleSpawns;
    //    for (int i = 0; i < possibleSpawns.Count; i++)
    //    {
    //        for (int j = 0; j < ineligibleSpawns.Count; j++)
    //        {
    //            if (possibleSpawns.Contains(ineligibleSpawns[j]))
    //                possibleSpawns.Remove(ineligibleSpawns[j]);
    //        }
    //    }
    //    return possibleSpawns;
    //}
