using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;

public class GridManager : MonoBehaviour
{
    public int GridWidth
    {
        get { return gridWidth; }
    }
    public int GridHeight
    {
        get { return gridHeight; }
    }


    public int BoundaryLeftActual
    {
        get { return -(gridWidth / 2); }
    }
    public int BoundaryRightActual
    {
        get
        {
            if (gridWidth % 2 == 0)
                return (gridWidth / 2) - 1;
            else
                return (gridWidth / 2);
        }
    }
    public int BoundaryTopActual
    {
        get
        {
            if (gridHeight % 2 == 0)
                return (gridHeight / 2) - 1;
            else
                return gridHeight / 2;
        }
    }
    public int BoundaryBottomActual
    {
        get { return -(gridHeight / 2); }
    }


    public int BoundaryLeftPlay
    {
        get { return BoundaryLeftActual + 1; }
    }
    public int BoundaryRightPlay
    {
        get { return BoundaryRightActual - 1; }
    }
    public int BoundaryTopPlay
    {
        get { return BoundaryTopActual - 1; }
    }
    public int BoundaryBottomPlay
    {
        get { return BoundaryBottomActual + 1; }
    }


    public List<Vector2Int> EligiblePerimeterSpawns
    {
        get
        {
            return eligiblePerimeterSpawns;
        }
    }
    public List<Vector2Int> EligibleInteriorSpawns
    {
        get
        {
            return eligibleInteriorSpawns;
        }
    }
    public List<Vector2Int> EligibleAllSpawns
    {
        get
        {
            eligibleAllSpawns.Clear();

            for(int i = 0; i < EligiblePerimeterSpawns.Count; i++)
            {
                eligibleAllSpawns.Add(eligiblePerimeterSpawns[i]);
            }
            for(int i = 0; i < EligibleInteriorSpawns.Count; i++)
            {
                eligibleAllSpawns.Add(EligibleInteriorSpawns[i]);
            }

            return eligibleAllSpawns;
        }
    }

    public Transform gridContainer;
    
    
    [SerializeField] private int gridSpacing = 1;
    [SerializeField] private GameObject debugGridPrefab;
    [SerializeField] private bool gridBlockLabels = false;


    [TitleGroup("LEVEL MANAGEMENT")]
    [SerializeField] private LevelData levelData;

    [SerializeField] private bool overrideLevelData;
    [ShowIf("overrideLevelData")] [SerializeField] private int inspectorGridWidth;
    [ShowIf("overrideLevelData")] [SerializeField] private int inspectorGridHeight;

    private int gridWidth;
    private int gridHeight;
    private int jumpFuelNeededForNextLevel;
    private int numberOfPhenomenaToSpawn;

    public System.Action OnUpdateBoard;
    public GridBlock[,] levelGrid;

    private List<Vector2Int> eligiblePerimeterSpawns = new List<Vector2Int>();
    private List<Vector2Int> eligibleInteriorSpawns = new List<Vector2Int>();
    private List<Vector2Int> eligibleAllSpawns = new List<Vector2Int>();

    public void Init()
    {
        InitializeGrid(debugGridPrefab, 0f);
    }

    /* V1
    private void ReadLevelData(int levelNumber)
    {
        if (levelData == null)
        {
            gridWidth = 10;
            gridHeight = 8;
        }
        else if (overrideLevelData)
        {
            gridWidth = inspectorGridWidth;
            gridHeight = inspectorGridHeight;
        }
        else
        {
            gridWidth = levelData.LevelTable[levelNumber].levelWidth;
            gridHeight = levelData.LevelTable[levelNumber].levelHeight;
            jumpFuelNeededForNextLevel = levelData.LevelTable[levelNumber].jumpFuelAmount;
            numberOfPhenomenaToSpawn = levelData.LevelTable[levelNumber].jumpFuelAmount;
        }

        // I could alter this method to have a return type of LevelData.LevelDataRow
        // This could then be fed into InitializeGrid as a parameter, so it would know how to create the grid
        // This would require nesting calls to these methods in another method tho ... which could be Init()
        // Then another method NextLevel() could be used to call ReadLevelData() and InitializeGrid()
    }
    */

    /*
    private LevelData.LevelDataRow GetLevelData(int levelNumber)
    {       
        if (levelData == null)
        {
            LevelData.LevelDataRow nextLevelData = new LevelData.LevelDataRow()
            {
                levelWidth = 10,
                levelHeight = 8,
                jumpFuelAmount = 4,
                numberOfPhenomenaToSpawn = 0
            };

            return nextLevelData;
        }
        else if (overrideLevelData)
        {

            LevelData.LevelDataRow nextLevelData = new LevelData.LevelDataRow()
            {
                levelWidth = inspectorGridWidth,
                levelHeight = inspectorGridHeight,
                jumpFuelAmount = 4,
                numberOfPhenomenaToSpawn = 0
            };

            return nextLevelData;
        }
        else
        {
            LevelData.LevelDataRow nextLevelData = new LevelData.LevelDataRow()
            {
                levelWidth = levelData.LevelTable[levelNumber].levelWidth,
                levelHeight = levelData.LevelTable[levelNumber].levelHeight,
                jumpFuelAmount = levelData.LevelTable[levelNumber].jumpFuelAmount,
                numberOfPhenomenaToSpawn = levelData.LevelTable[levelNumber].jumpFuelAmount
            };

            return nextLevelData;
        }

        // Creates new LevelData.LevelDataRow objects to pass into InitializeGrid()
        // I've just realized that this whole method isn't really needed ... motherfucker
        // There goes 2 hours thinking about architecture for something that isn't even needed dammit!
        // Anyway, I think InitializeGrid() can just accept a LevelData.LevelDataRow parameter, which will be provided by:
        //  - Init()
        //  - NextLevel()
    }
    */

    private void InitializeGrid()
    {
        levelGrid = new GridBlock[gridWidth, gridHeight];
        Debug.Log("Object: [levelGrid] created.");
        Debug.Log(levelGrid.Length);

        // Iterate through columns.
        for (int x = 0; x < gridWidth; x++) {
            // Iterate through rows.
            for (int y = 0; y < gridHeight; y++) {
                // Instantiate a GridBlock at each index in the 2D array
                levelGrid[x, y] = new GridBlock(x, y);

                // Add Vector2Int objects
                levelGrid[x, y].location = new Vector2Int(x, y);

                // Update canSpawn property
                if (levelGrid[x, y].location.x == 0 || levelGrid[x, y].location.x == gridWidth - 1) {
                    levelGrid[x, y].canSpawn = true;
                }

                if (levelGrid[x, y].location.y == 0 || levelGrid[x, y].location.y == gridHeight - 1) {
                    levelGrid[x, y].canSpawn = true;
                }
            }
        }
    }
    private void InitializeGrid(GameObject gridPoint, float offset)
    {
        /*  SUMMARY
         *  - Instantiate levelGrid
         *  - Populate levelGrid elements with GridBlock cells
         *  - Update levelGrid[x, y].location with Vector2Int
         *  - Update levelGrid[x, y].canSpawn
         */

        levelGrid = new GridBlock[gridWidth, gridHeight];

        int locationX = BoundaryLeftActual;
        int locationY = BoundaryBottomActual;

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                if (j == 0) locationY = BoundaryBottomActual;

                levelGrid[i, j] = new GridBlock(locationX, locationY);
                //levelGrid[i, j].location = new Vector2Int(locationX, locationY);

                if ((locationX == BoundaryLeftActual || locationX == BoundaryRightActual) && locationY != BoundaryTopActual && locationY != BoundaryBottomActual)
                {
                    eligiblePerimeterSpawns.Add(levelGrid[i, j].location);
                }
                else if ((locationX != BoundaryLeftActual && locationX != BoundaryRightActual) && (locationY == BoundaryTopActual || locationY == BoundaryBottomActual))
                {
                    eligiblePerimeterSpawns.Add(levelGrid[i, j].location);
                }
                else
                {
                    eligibleInteriorSpawns.Add(levelGrid[i, j].location);
                }

                GameObject point = Instantiate
                (
                    gridPoint,
                    new Vector3(locationX + offset, locationY + offset, 0f),
                    Quaternion.identity,
                    gridContainer
                );

                levelGrid[i, j].DebugRenderPoint = point;

/*              OLD CODE TO ALTER LEVELGRID POINT COLORS
                if (locationX == BoundaryLeftActual || locationX == BoundaryRightActual)
                {
                    //point.GetComponent<Renderer>().material.color = Color.green;
                }
                    
                else if (locationY == BoundaryBottomActual || locationY == BoundaryTopActual)
                    //point.GetComponent<Renderer>().material.color = Color.green;
*/
                locationY += 1;
            }

            locationX += 1;
        }
        Debug.Log("Object: [levelGrid] successfully created.");
        Debug.LogFormat("levelGrid d1 length: {0} \n levelgrid d2 length: {1}", levelGrid.GetLength(0), levelGrid.GetLength(1));
    }

    
    public Vector3 GridToWorld(Vector2Int gridLocation)
    {
        return new Vector3
        (
            gridLocation.x / gridSpacing,
            gridLocation.y / gridSpacing,
            0f
        );
    }
    public Vector2Int WorldToGrid(Vector3 worldLocation)
    {
        return new Vector2Int
        (
            (int)worldLocation.x * gridSpacing,
            (int)worldLocation.y * gridSpacing
        );

    }


    public GridBlock FindGridBlockContainingObject(GameObject gameObject)
    {
        for (int x = 0; x < levelGrid.GetLength(0); x++)
        {
            for (int y = 0; y < levelGrid.GetLength(1); y++)
            {
                int objectsOnBlock = levelGrid[x, y].objectsOnBlock.Count;

                if (objectsOnBlock > 0)
                {
                    for (int z = 0; z < objectsOnBlock; z++)
                    {
                        if (levelGrid[x, y].objectsOnBlock[z] == gameObject) return levelGrid[x, y];
                    }
                }
            }
        }

        Debug.LogError("Game Object not found!");
        return null;
    }
    public GridBlock FindGridBlockByLocation(Vector2Int location)
    {
        for (int i = 0; i < levelGrid.GetLength(0); i++) {
            for (int j = 0; j < levelGrid.GetLength(1); j++) {
                if (levelGrid[i, j].location == location) {
                    return levelGrid[i, j];
                }
            }
        }

        return null;
    }


    public bool CheckIfGridBlockInBounds(Vector2Int gridLocation)
    {
        if
        (
            gridLocation.x >= BoundaryLeftPlay &&
            gridLocation.x <= BoundaryRightPlay &&
            gridLocation.y <= BoundaryTopPlay &&
            gridLocation.y >= BoundaryBottomPlay
        ) return true;

        else return false;
    }
    public bool CheckIfGridBlockIsAvailable(Vector2Int gridLocation)
    {
        GridBlock block = FindGridBlockByLocation(gridLocation);
        if (block == null) return false;
        return block.IsAvailableForPlayer;
    }


    public void AddObjectToGrid(GameObject gameObject, Vector2Int gridLocation)
    {
        for (int i = 0; i < levelGrid.GetLength(0); i++) {
            for (int j = 0; j < levelGrid.GetLength(1); j++) {
                if (levelGrid[i, j].location == gridLocation) {
                    //GridBlock destination = levelGrid[gridLocation.x, gridLocation.y];
                    //destination.objectsOnBlock.Add(gameObject);
                    levelGrid[i, j].objectsOnBlock.Add(gameObject);
                    return;
                }
            }
        }
    }
    public void RemoveObjectFromGrid(GameObject gameObject, Vector2Int gridLocation)
    {
        GridBlock origin = FindGridBlockByLocation(gridLocation);

        if (origin.objectsOnBlock.Count > 0) {

            for (int i = origin.objectsOnBlock.Count - 1; i >= 0; i--) {
                if (gameObject == origin.objectsOnBlock[i]) {
                    origin.objectsOnBlock.RemoveAt(i);
                    return;
                }
            }
        }
    }


    public List<Vector2Int> GetGridPath(Vector2Int origin, Vector2Int direction)
    {
        List<Vector2Int> gridBlockPath = new List<Vector2Int>();

        if (direction == Vector2Int.up)     // x stays the same, y increments positively
        {
            Vector2Int targetLocation = origin + Vector2Int.up;
            for (int i = origin.y; i < BoundaryTopActual; i++)
            {
                gridBlockPath.Add(targetLocation);
                targetLocation += Vector2Int.up;
            }
        }
        else if (direction == Vector2Int.down)   // x stays the same, y increments negatively
        {
            Vector2Int targetLocation = origin + Vector2Int.down;
            for (int i = origin.y; i > BoundaryBottomActual; i--)
            {
                gridBlockPath.Add(targetLocation);
                targetLocation += Vector2Int.down;
            }
        }
        else if (direction == Vector2Int.left)   // y stays the same, x increments negatively
        {
            Vector2Int targetLocation = origin + Vector2Int.left;
            for (int i = origin.x; i > BoundaryLeftActual; i--)
            {
                gridBlockPath.Add(targetLocation);
                targetLocation += Vector2Int.left;
            }
        }
        else if (direction == Vector2Int.right)   // y stays the same, x increments positively
        {
            Vector2Int targetLocation = origin + Vector2Int.right; 
            for (int i = origin.x; i < BoundaryRightActual; i++)
            {
                gridBlockPath.Add(targetLocation);  
                targetLocation += Vector2Int.right; 
            }
        }

        return gridBlockPath;
    }
    public List<GridBlock> GetAvailableGridBlockForMovement(Vector2Int origin, Vector2Int direction)
    {
        List<GridBlock> possibleDestinations = new List<GridBlock>();

        if(origin.x + Vector2Int.left.x >= BoundaryLeftPlay)
            possibleDestinations.Add(FindGridBlockByLocation(origin + Vector2Int.left));

        if(origin.x + Vector2Int.right.x <= BoundaryRightPlay)
            possibleDestinations.Add(FindGridBlockByLocation(origin + Vector2Int.right));

        if (origin.y + Vector2Int.up.y <= BoundaryTopPlay)
            possibleDestinations.Add(FindGridBlockByLocation(origin + Vector2Int.up));

        if (origin.y + Vector2Int.down.y >= BoundaryBottomPlay)
            possibleDestinations.Add(FindGridBlockByLocation(origin + Vector2Int.down));

        return possibleDestinations;
    }
    public List<Vector2Int> GetSpawnLocations(SpawnRule rules)
    {
        Debug.Log("GridManager.GetSpawnLocations() called.");

        List<Vector2Int> availableSpawnLocations = new List<Vector2Int>();
        List<Vector2Int> ineligibleSpawnLocations = new List<Vector2Int>();

        Debug.LogFormat("Number of available spawn locations at start: {0}", availableSpawnLocations.Count);

        for (int i = 0; i < GridWidth; i++)
        {
            for (int j = 0; j < GridHeight; j++)
            {
                if (rules.spawnRegion == SpawnRule.SpawnRegion.Perimeter)
                {  
                    //Debug.LogFormat("Location is perimeter eligible: {0} .", EligiblePerimeterSpawns.Contains(levelGrid[i, j].location));
                    if (EligiblePerimeterSpawns.Contains(levelGrid[i, j].location))
                        availableSpawnLocations.Add(levelGrid[i, j].location);
                }
                else if (rules.spawnRegion == SpawnRule.SpawnRegion.Interior)
                {
                    //Debug.LogFormat("Location is interior eligible: {0} .", EligibleInteriorSpawns.Contains(levelGrid[i, j].location));
                    if (EligibleInteriorSpawns.Contains(levelGrid[i, j].location))
                        availableSpawnLocations.Add(levelGrid[i, j].location);
                }

                for (int k = 0; k < levelGrid[i, j].objectsOnBlock.Count; k++)
                {
                    Hazard currentHazard = levelGrid[i, j].objectsOnBlock[k].GetComponent<Hazard>();
                    MovePattern currentHazardMove = levelGrid[i, j].objectsOnBlock[k].GetComponent<MovePattern>();

                    if (currentHazard != null && currentHazardMove != null)
                    {
                        if (rules.spawnRegion == SpawnRule.SpawnRegion.Perimeter)
                        {
                            if (rules.avoidHazardPaths)
                            {
                                if (currentHazardMove.DirectionOnGrid == Vector2Int.up)
                                {
                                    // Disable spawning on opposing GridBlock at boundary
                                    Vector2Int boundaryLocationToRemove = new Vector2Int((int)currentHazard.currentWorldLocation.x, BoundaryTopActual);
                                    Debug.LogFormat("Adding {0} to the ineligible spawn list.", boundaryLocationToRemove.ToString());
                                    ineligibleSpawnLocations.Add(boundaryLocationToRemove);

                                    // Disable spawning immediately in front of current hazard on left playable boundary
                                    if (boundaryLocationToRemove.x == BoundaryLeftPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.left;
                                        Debug.LogFormat("Adding {0} to the ineligible spawn list.", locationToRemove.ToString());
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.up;
                                        Debug.LogFormat("Adding {0} to the ineligible spawn list.", locationToRemove.ToString());
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                    // Disable spawning immediately in front of current hazard on right playable boundary
                                    else if (boundaryLocationToRemove.x == BoundaryRightPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.right;
                                        Debug.LogFormat("Adding {0}sms to the ineligible spawn list.", locationToRemove.ToString());
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.up;
                                        Debug.LogFormat("Adding {0] to the ineligible spawn list.", locationToRemove.ToString());
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                }
                                else if (currentHazardMove.DirectionOnGrid == Vector2Int.down)
                                {
                                    // Disable spawning on opposing GridBlock at boundary
                                    Vector2Int boundaryLocationToRemove = new Vector2Int((int)currentHazard.currentWorldLocation.x, BoundaryBottomActual);
                                    ineligibleSpawnLocations.Add(boundaryLocationToRemove);

                                    // Disable spawning immediately in front of current hazard on left playable boundary
                                    if (boundaryLocationToRemove.x == BoundaryLeftPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.left;
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.down;
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                    // Disable spawning immediately in front of current hazard on right playable boundary
                                    else if (boundaryLocationToRemove.x == BoundaryRightPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.right;
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.down;
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                }
                                else if (currentHazardMove.DirectionOnGrid == Vector2Int.left)
                                {
                                    // Disable spawning on opposing GridBlock at boundary
                                    Vector2Int boundaryLocationToRemove = new Vector2Int(BoundaryLeftActual, (int)currentHazard.currentWorldLocation.y);
                                    ineligibleSpawnLocations.Add(boundaryLocationToRemove);

                                    // Disable spawning immediately in front of current hazard on top playable boundary
                                    if (boundaryLocationToRemove.y == BoundaryTopPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.up;
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.left;
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                    // Disable spawning immediately in front of current hazard on bottom playable boundary
                                    else if (currentHazardMove.CanMoveThisTurn && boundaryLocationToRemove.y == BoundaryBottomPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.down;
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.left;
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                }
                                else if (currentHazardMove.DirectionOnGrid == Vector2Int.right)
                                {
                                    // Disable spawning on opposing GridBlock at boundary
                                    Vector2Int boundaryLocationToRemove = new Vector2Int(BoundaryRightActual, (int)currentHazard.currentWorldLocation.y);
                                    ineligibleSpawnLocations.Remove(boundaryLocationToRemove);

                                    // Disable spawning in immediate vicinity of current hazard along top playable boundary
                                    if (boundaryLocationToRemove.y == BoundaryTopPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.up;
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.right;
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                    // Disable spawning immediately in front of current hazard on bottom playable boundary
                                    else if (boundaryLocationToRemove.y == BoundaryBottomPlay)
                                    {
                                        Vector2Int locationToRemove = WorldToGrid(currentHazard.currentWorldLocation) + Vector2Int.down;
                                        ineligibleSpawnLocations.Add(locationToRemove);

                                        locationToRemove += Vector2Int.right;
                                        ineligibleSpawnLocations.Add(locationToRemove);
                                    }
                                }
                            }

                            if (rules.avoidAdjacentToPlayer == true)
                            {
                                Vector3 playerLocation = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().currentWorldLocation;
                            }
                        }
                        else if (rules.spawnRegion == SpawnRule.SpawnRegion.Interior)
                        {
                            if (EligibleInteriorSpawns.Contains(levelGrid[i, j].location))
                                availableSpawnLocations.Add(levelGrid[i, j].location);

                            Vector2Int currentHazardGridLocation = WorldToGrid(currentHazard.currentWorldLocation);
                            ineligibleSpawnLocations.Add(currentHazardGridLocation);

                            if (rules.avoidHazardPaths)
                            {
                                Vector2Int direction = currentHazardMove.DirectionOnGrid;
                                List<Vector2Int> gridLocationsToRemove = GetGridPath(currentHazardGridLocation, direction);
                            }

                            if (rules.avoidAdjacentToPlayer)
                            {

                            }
                        }
                    }
                }
            }
        }

        Debug.LogFormat("Number of available spawn locations at end: {0}", availableSpawnLocations.Count);

        for (int i = 0; i < ineligibleSpawnLocations.Count; i++)
        {
            if (availableSpawnLocations.Contains(ineligibleSpawnLocations[i]))
            {
                availableSpawnLocations.Remove(ineligibleSpawnLocations[i]);
            }
        }
        return availableSpawnLocations;
    }



    private void OnDrawGizmos()
    {
        if (gridBlockLabels == true)
        {
            Gizmos.color = Color.blue;
            if (levelGrid != null)
            {
                foreach (GridBlock block in levelGrid)
                {    
                    foreach (GameObject occupant in block.objectsOnBlock)
                    {
                        if (occupant != null)
                        {
                            GUIStyle GridObjectOccupantGizmoText = new GUIStyle();
                            GridObjectOccupantGizmoText.normal.textColor = Color.red;
                            GridObjectOccupantGizmoText.fontSize = 20;
                            //GridObjectOccupantGizmoText.normal.background = Texture2D.blackTexture;

                            //Handles.Label(GridToWorld(block.location), occupant.name);
                            Handles.Label(GridToWorld(block.location), occupant.name, GridObjectOccupantGizmoText);
                        }
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class SpawnRule {
        public enum SpawnRegion { Anywhere, Perimeter, Interior }
        public SpawnRegion spawnRegion = SpawnRegion.Anywhere;
        public bool avoidHazardPaths = false;
        public bool avoidAdjacentToPlayer = false;
        public bool requiresOrientation = false;
    }

}