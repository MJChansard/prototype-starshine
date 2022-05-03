using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SpawnManager : MonoBehaviour
{
    //  # INSPECTOR
    [BoxGroup("GENERAL COMPONENT CONFIGURATION", centerLabel: true)]
    [SerializeField] bool VerboseConsole;

    [BoxGroup("LIBRARY", centerLabel: true)]
    [SerializeField] GridObject[] gridObjectPrefabs;

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
    public bool CustomSpawnSequenceExist { get { return customSpawnSequences.Count == 0; } }

    //  # FIELDS
    List<GridObject> hazards    = new List<GridObject>();
    List<GridObject> loot       = new List<GridObject>();
    List<GridObject> phenomena  = new List<GridObject>();
    
    List<Vector2Int> eligiblePerimeterSpawns    = new List<Vector2Int>();
    List<Vector2Int> eligibleInteriorSpawns     = new List<Vector2Int>();
    List<Vector2Int> eligibleAllSpawns          = new List<Vector2Int>();

    List<SpawnWave> customSpawnSequences = new List<SpawnWave>();
    Queue<SpawnRecord> spawnQueue = new Queue<SpawnRecord>();

    LevelRecord thisLevel;


    void Init()
    {
        for (int i = 0; i < gridObjectPrefabs.Length; i++)
        {
            if (gridObjectPrefabs[i] is Hazard)
                hazards.Add(gridObjectPrefabs[i]);

            if (gridObjectPrefabs[i] is Phenomena)
                phenomena.Add(gridObjectPrefabs[i]);
        }

        if (thisLevel == null)
            Debug.Log("No data available for current level.");
        else
        {
            int leftSpawn = 0;
        }

        FindEligibleSpawns();
    }

    void FindEligibleSpawns()
    {
        int locationX = thisLevel.BoundaryLeftActual;
        int locationY = thisLevel.BoundaryBottomActual;
        for (int i = 0; i < thisLevel.width; i++)
        {
            for (int j = 0; j < thisLevel.height; j++)
            {
                if (j == 0) locationY = thisLevel.BoundaryBottomActual;

                Vector2Int gridLocation = new Vector2Int(locationX, locationY);

                if ((locationX == thisLevel.BoundaryLeftActual || locationX == thisLevel.BoundaryRightActual) && locationY != thisLevel.BoundaryTopActual && locationY != thisLevel.BoundaryBottomActual)
                {
                    eligiblePerimeterSpawns.Add(gridLocation);
                }
                else if ((locationX != thisLevel.BoundaryLeftActual && locationX != thisLevel.BoundaryRightActual) && (locationY == thisLevel.BoundaryTopActual || locationY == thisLevel.BoundaryBottomActual))
                {
                    eligiblePerimeterSpawns.Add(gridLocation);
                }
                else if ((locationX > thisLevel.BoundaryLeftActual && locationX < thisLevel.BoundaryRightActual && locationY < thisLevel.BoundaryTopActual && locationY > thisLevel.BoundaryBottomActual))
                {
                    eligibleInteriorSpawns.Add(gridLocation);
                }

                locationY += 1;
            }

            locationX += 1;
        }
    }
    void CreateSpawnRecord()
    {
        SpawnRecord newSpawn = SpawnRecord.CreateInstance<SpawnRecord>();

    }
    void CreateSpawnWave()
    {

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
}
