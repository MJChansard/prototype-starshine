using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour {
    #region Public Properties
    public int GridWidth {
        get { return gridWidth; }
    }
    public int GridHeight {
        get { return gridHeight; }
    }


    public int BoundaryLeftActual {
        get { return -(gridWidth / 2);
        }
    }
    public int BoundaryRightActual {
        get {
            if (gridWidth % 2 == 0)
                return (gridWidth / 2) - 1;
            else
                return (gridWidth / 2);
        }
    }
    public int BoundaryTopActual {
        get {
            if (gridHeight % 2 == 0)
                return (gridHeight / 2) - 1;
            else
                return gridHeight / 2;
        }
    }
    public int BoundaryBottomActual {
        get { return -(gridHeight / 2); }
    }


    public int BoundaryLeftPlay {
        get { return BoundaryLeftActual + 1; }
    }
    public int BoundaryRightPlay {
        get { return BoundaryRightActual - 1; }
    }
    public int BoundaryTopPlay {
        get { return BoundaryTopActual - 1; }
    }
    public int BoundaryBottomPlay {
        get { return BoundaryBottomActual + 1; }
    }

    public Transform gridContainer;

    #endregion

    #region Inspector Attributes    
    [SerializeField] private int gridSpacing = 1;

    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 8;

    [SerializeField] private GameObject debugGridPrefab;
    [SerializeField] private bool gridBlockLabels = false;
    #endregion

    public System.Action OnUpdateBoard;
    public GridBlock[,] levelGrid;

    public void Init() {
        InitializeGrid(debugGridPrefab, 0f);
        ResetSpawns();
    }


    private void InitializeGrid() {
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

    /*
        private void InitializeGrid(GameObject gridPoint, float offset)
        {
            levelGrid = new GridBlock[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    levelGrid[x, y] = new GridBlock(x, y);
                    levelGrid[x, y].location = new Vector2Int(x, y);

                    // Display grid.
                    GameObject point = Instantiate
                    (
                        gridPoint,
                        new Vector3(x + offset, y + offset, 0f),
                        Quaternion.identity,
                        gridContainer
                    );

                    // Update canSpawn property
                    if (levelGrid[x, y].location.x == 0 || levelGrid[x, y].location.x == gridWidth - 1)
                    {
                        levelGrid[x, y].canSpawn = true;
                        point.GetComponent<Renderer>().material.color = Color.green;
                    }

                    if (levelGrid[x, y].location.y == 0 || levelGrid[x, y].location.y == gridHeight - 1)
                    {
                        levelGrid[x, y].canSpawn = true;
                        point.GetComponent<Renderer>().material.color = Color.green;
                    }
                }
            }

            Debug.Log("Object: [levelGrid] successfully created.");
        }
    */
    private void InitializeGrid(GameObject gridPoint, float offset) {
        /*  SUMMARY
         *  - Instantiate levelGrid
         *  - Populate levelGrid elements with GridBlock cells
         *  - Update levelGrid[x, y].location with Vector2Int
         *  - Update levelGrid[x, y].canSpawn
         */

        levelGrid = new GridBlock[gridWidth, gridHeight];

        int locationX = BoundaryLeftActual;
        int locationY = BoundaryBottomActual;

        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                if (j == 0) locationY = BoundaryBottomActual;

                levelGrid[i, j] = new GridBlock(locationX, locationY);
                levelGrid[i, j].location = new Vector2Int(locationX, locationY);

                GameObject point = Instantiate
                (
                    gridPoint,
                    new Vector3(locationX + offset, locationY + offset, 0f),
                    Quaternion.identity,
                    gridContainer
                );

                levelGrid[i, j].DebugRenderPoint = point;

                if (locationX == BoundaryLeftActual || locationX == BoundaryRightActual)
                    point.GetComponent<Renderer>().material.color = Color.green;
                else if (locationY == BoundaryBottomActual || locationY == BoundaryTopActual)
                    point.GetComponent<Renderer>().material.color = Color.green;

                locationY += 1;
            }

            locationX += 1;
        }
        Debug.Log("Object: [levelGrid] successfully created.");
        Debug.LogFormat("levelGrid d1 length: {0} \n levelgrid d2 length: {1}", levelGrid.GetLength(0), levelGrid.GetLength(1));
    }

    public void ResetSpawns() {
        for (int i = 0; i < levelGrid.GetLength(0); i++) {
            for (int j = 0; j < levelGrid.GetLength(1); j++) {
                if (levelGrid[i, j].location.x == BoundaryLeftActual || levelGrid[i, j].location.x == BoundaryRightActual) {
                    levelGrid[i, j].canSpawn = true;
                    levelGrid[i, j].DebugRenderPoint.GetComponent<Renderer>().material.color = Color.green;
                }


                if (levelGrid[i, j].location.y == BoundaryBottomActual || levelGrid[i, j].location.y == BoundaryTopActual) {
                    levelGrid[i, j].canSpawn = true;
                    levelGrid[i, j].DebugRenderPoint.GetComponent<Renderer>().material.color = Color.green;
                }
            }
        }

        // Disable corners
        levelGrid[0, 0].canSpawn = false;
        levelGrid[0, GridHeight - 1].canSpawn = false;
        levelGrid[GridWidth - 1, 0].canSpawn = false;
        levelGrid[GridWidth - 1, GridHeight - 1].canSpawn = false;

    }

    public void DeactivateGridBlockSpawn(Vector2Int gridBlockLocation) {
        Debug.LogFormat("Disabling GridBlock: {0}", gridBlockLocation);

        GridBlock gridBlock = FindGridBlockByLocation(gridBlockLocation);

        if (gridBlock.canSpawn == true) {
            gridBlock.canSpawn = false;
            gridBlock.DebugRenderPoint.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public Vector3 GridToWorld(Vector2Int gridLocation) {
        return new Vector3
        (
            gridLocation.x / gridSpacing,
            gridLocation.y / gridSpacing,
            0f
        );
    }

    public Vector2Int WorldToGrid(Vector3 worldLocation) {
        return new Vector2Int
        (
            (int)worldLocation.x * gridSpacing,
            (int)worldLocation.y * gridSpacing
        );

    }


    public GridBlock FindGridBlockContainingObject(GameObject gameObject) {
        for (int x = 0; x < levelGrid.GetLength(0); x++) {
            for (int y = 0; y < levelGrid.GetLength(1); y++) {
                int objectsOnBlock = levelGrid[x, y].objectsOnBlock.Count;
                if (objectsOnBlock > 0) {
                    for (int z = 0; z < objectsOnBlock; z++) {
                        if (levelGrid[x, y].objectsOnBlock[z] == gameObject) return levelGrid[x, y];
                    }
                }
            }
        }

        Debug.LogError("Game Object not found!");
        return null;
    }

    public GridBlock FindGridBlockByLocation(Vector2Int location) {
        for (int i = 0; i < levelGrid.GetLength(0); i++) {
            for (int j = 0; j < levelGrid.GetLength(1); j++) {
                if (levelGrid[i, j].location == location) {
                    return levelGrid[i, j];
                }
            }
        }

        return null;
    }


    public bool CheckIfGridBlockInBounds(Vector2Int gridLocation) {
        if
        (
            gridLocation.x >= BoundaryLeftPlay &&
            gridLocation.x <= BoundaryRightPlay &&
            gridLocation.y <= BoundaryTopPlay &&
            gridLocation.y >= BoundaryBottomPlay
        ) return true;

        else return false;
    }

    public bool CheckIfGridBlockIsAvailable(Vector2Int gridLocation) {
        GridBlock block = FindGridBlockByLocation(gridLocation);
        if (block == null) return false;
        return block.IsAvailableForPlayer;
    }


    public void AddObjectToGrid(GameObject gameObject, Vector2Int gridLocation) {
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

    public void RemoveObjectFromGrid(GameObject gameObject, Vector2Int gridLocation) {
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


    private void OnDrawGizmos() {
        if (gridBlockLabels == true) {
            Gizmos.color = Color.blue;
            if (levelGrid != null) {
                foreach (GridBlock block in levelGrid) {
                    if (!block.IsAvailableForPlayer) {
                        foreach (GameObject occupant in block.objectsOnBlock) {
                            if (occupant != null) {
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
    }

    [System.Serializable]
    public class SpawnRule {
        public enum SpawnRegion { Anywhere, Perimeter, Interior }
        public SpawnRegion spawnRegion = SpawnRegion.Anywhere;
        public bool avoidHazardPaths = false;
        public bool avoidAdjacentToPlayer = false;
    }

}