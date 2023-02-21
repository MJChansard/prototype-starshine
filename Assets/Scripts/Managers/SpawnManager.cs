using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using tripolygon.UModeler;
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

    int CountAvailableBorderSpawns
    {
        get
        {
            int count = 0;
            if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Top))
                count += availableBorderSpawns[GridBorder.Top].Count;

            if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Bottom))
                count += availableBorderSpawns[GridBorder.Bottom].Count;

            if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Left))
                count += availableBorderSpawns[GridBorder.Left].Count;

            if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Right))
                count += availableBorderSpawns[GridBorder.Right].Count;
            
            return count;
        }
    }




    //  # FIELDS
    List<GridObject> hazards = new();
    List<GridObject> loot = new();
    List<GridObject> phenomena;
    List<GridObject> stations;

    List<Vector2Int> eligibleInteriorSpawns = new();
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
            EnqueueNewSpawnWave(1);
        }
    }

    void FindEligibleSpawns()
    {
        for (int x = thisLevel.BoundaryLeftActual; x <= thisLevel.BoundaryRightActual; x++)
        {
            for (int y = thisLevel.BoundaryBottomActual; y < thisLevel.BoundaryTopActual; y++)
            {
                Vector2Int gridLocation = new Vector2Int(x, y);

                if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Left) && x == thisLevel.BoundaryLeftActual && y != thisLevel.BoundaryTopActual && y != thisLevel.BoundaryBottomActual)
                {
                    availableBorderSpawns[GridBorder.Left].Add(gridLocation);
                }
                else if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Right) && x == thisLevel.BoundaryRightActual && y != thisLevel.BoundaryTopActual && y != thisLevel.BoundaryBottomActual)
                {
                    availableBorderSpawns[GridBorder.Right].Add(gridLocation);
                }
                else if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Top) && x != thisLevel.BoundaryLeftActual && x != thisLevel.BoundaryRightActual && y == thisLevel.BoundaryTopActual)
                {
                    availableBorderSpawns[GridBorder.Top].Add(gridLocation);
                }
                else if (thisLevel.bordersEligibleForSpawn.Contains(GridBorder.Bottom) && x != thisLevel.BoundaryLeftActual && x != thisLevel.BoundaryRightActual && y == thisLevel.BoundaryBottomActual)
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
        if (CountAvailableBorderSpawns > 0)
        {
            for (int w = 0; w < waveCount; w++)
            {
                int maxObjectsForWave = CountAvailableBorderSpawns >= thisLevel.maxObjectsPerWave ? thisLevel.maxObjectsPerWave : CountAvailableBorderSpawns;
                int numberOfObjectsInWave = Random.Range(thisLevel.minObjectsPerWave, maxObjectsForWave + 1);
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
                        GridBorder selected;

                        List<GridBorder> _listOfBorders = new();
                        foreach (GridBorder b in thisLevel.bordersEligibleForSpawn)
                        {
                            if (availableBorderSpawns[b].Count > 0)
                                _listOfBorders.Add(b);
                        }

                        if (_listOfBorders.Count == 0)
                            return;
                        else if (_listOfBorders.Count == 1)
                            selected = _listOfBorders[0];
                        else
                        {
                            int x = Random.Range(0, thisLevel.bordersEligibleForSpawn.Length);
                            selected = thisLevel.bordersEligibleForSpawn[x];
                        }
                        
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
    }

    public void ReclaimSpawn(Vector2Int location)
    {
        if (location.x == thisLevel.BoundaryLeftActual)
        {
            availableBorderSpawns[GridBorder.Left].Add(location);

            Vector2Int opposite = new Vector2Int(thisLevel.BoundaryRightActual, location.y);
            if (!availableBorderSpawns[GridBorder.Right].Contains(opposite))
                availableBorderSpawns[GridBorder.Right].Add(opposite);
        }
        else if (location.x == thisLevel.BoundaryRightActual)
        {
            availableBorderSpawns[GridBorder.Right].Add(location);

            Vector2Int opposite = new Vector2Int(thisLevel.BoundaryLeftActual, location.y);
            if (!availableBorderSpawns[GridBorder.Left].Contains(opposite))
                availableBorderSpawns[GridBorder.Left].Add(opposite);
        }
        else if (location.y == thisLevel.BoundaryTopActual)
        {
            availableBorderSpawns[GridBorder.Top].Add(location);

            Vector2Int opposite = new Vector2Int(location.x, thisLevel.BoundaryBottomActual);
            if (!availableBorderSpawns[GridBorder.Bottom].Contains(opposite))
                availableBorderSpawns[GridBorder.Bottom].Add(opposite);
        }
        else if (location.y == thisLevel.BoundaryBottomActual)
        {
            availableBorderSpawns[GridBorder.Bottom].Add(location);

            Vector2Int opposite = new Vector2Int(location.x, thisLevel.BoundaryTopActual);

            if (!availableBorderSpawns[GridBorder.Top].Contains(opposite))
                availableBorderSpawns[GridBorder.Top].Add(opposite);
        }
            
    }
}