using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region Public Properties
    
    public GridBlock[,] levelGrid;
    public int gridWidth = 10;  //QUESTION: Make these private fields but have a public property?
    public int gridHeight = 8;

    public List<GameObject> hazards = new List<GameObject>();
    //public List<int> hazards = new List<int>();

    #endregion


    #region Inspector Attributes    
    [SerializeField]
    private int gridSpacing = 1;

    [SerializeField]
    private GameObject debugGridPrefab;

    [SerializeField]
    GameObject playerPrefab;
    #endregion


    #region Private Attributes
    GameObject player;
    PlayerManager playerManager;
    GameObject gameController;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController");

        InitializeGrid(debugGridPrefab, 0f);

        player = Instantiate(playerPrefab);
        PlaceObject(player, new Vector2Int(5, 4));
        playerManager = player.GetComponent<PlayerManager>();
    }


    // Update is called once per frame
    void Update()
    {

    }


    private void InitializeGrid()
    {
        levelGrid = new GridBlock[gridWidth, gridHeight];
        Debug.Log("Object: [levelGrid] created.");
        Debug.Log(levelGrid.Length);

        // Iterate through columns.
        for (int x = 0; x < gridWidth; x++)
        {
            // Iterate through rows.
            for (int y = 0; y < gridHeight; y++)
            {
                // Instantiate a GridBlock at each index in the 2D array
                levelGrid[x, y] = new GridBlock(x, y);

                // Add Vector2Int objects
                levelGrid[x, y].location = new Vector2Int(x, y);

                // Update canSpawn property
                if (levelGrid[x, y].location.x == 0 || levelGrid[x, y].location.x == gridWidth - 1)
                {
                    levelGrid[x, y].canSpawn = true;
                }

                if (levelGrid[x, y].location.y == 0 || levelGrid[x, y].location.y == gridHeight - 1)
                {
                    levelGrid[x, y].canSpawn = true;
                }
            }
        }

        Debug.Log("Object: [levelGrid] successfully created.");
    }


    private void InitializeGrid(GameObject gridPoint, float offset)
    {
        levelGrid = new GridBlock[gridWidth, gridHeight];
        Debug.Log("Object: [levelGrid] created.");
        Debug.Log(levelGrid.Length);

        // Iterate through columns.
        for (int x = 0; x < gridWidth; x++)
        {
            // Iterate through rows.
            for (int y = 0; y < gridHeight; y++)
            {
                // Instantiate a GridBlock at each index in the 2D array
                levelGrid[x, y] = new GridBlock(x, y);

                // Add Vector2Int objects
                levelGrid[x, y].location = new Vector2Int(x, y);

                // Update canSpawn property
                if(levelGrid[x, y].location.x == 0 || levelGrid[x, y].location.x == gridWidth - 1)
                {
                    levelGrid[x, y].canSpawn = true;
                }

                if (levelGrid[x, y].location.y == 0 || levelGrid[x, y].location.y == gridHeight - 1)
                {
                    levelGrid[x, y].canSpawn = true;
                }

                // Create Debug grid.  Note y and x are inversed in the Vector3 intentionally
                Instantiate
                (
                    gridPoint,
                    new Vector3(x + offset, y + offset, 0f),
                    Quaternion.identity,
                    gameController.transform
                );

            }
        }

        Debug.Log("Object: [levelGrid] successfully created.");
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


    public void PlaceObject(GameObject gameObject, Vector2Int position)
    {
        GridBlock target = levelGrid[position.x, position.y];  // Watch for out of bounds values
        if (!target.isOccupied)
        {
            target.isOccupied = true;
            target.objectOnBlock = gameObject;

            gameObject.transform.position = GridToWorld(position);
        }

        return;
    }

    public void RequestMoveRelative(GameObject gameObject, Vector2Int delta)
    {
        for (int x = 0; x < levelGrid.GetLength(0); x++)
        {
            // Iterate through rows.
            for (int y = 0; y < levelGrid.GetLength(1); y++)
            {
                if (levelGrid[x, y].objectOnBlock == gameObject)
                {
                    Debug.Log("GridBlock located.");
                    Vector2Int position = new Vector2Int(x, y);
                    RequestMove(levelGrid[x, y].objectOnBlock, position, position + delta);
                    return;
                }
            }
        }

        Debug.LogError("Game Object not found!");
    }

    public void RequestMove(GameObject gameObject, Vector2Int from, Vector2Int to)
    {
        Debug.Log(gameObject.name + " requesting move " + from + " to "+ to + ".");
        
        GridBlock fromBlock = levelGrid[from.x, from.y];
        GridBlock toBlock;

        // QUESTION: Is nesting ifs better than multiple conditionals?
        // Ensure destination exists within the grid
        if (to.x >= 0 && to.x <= levelGrid.GetLength(0) && to.y >= 0 && to.y <= levelGrid.GetLength(1))
        {
            toBlock = levelGrid[to.x, to.y];

            if (!toBlock.isOccupied)
            {
                PerformMove(gameObject, fromBlock, toBlock);
            }
            else
            {
                Debug.Log("Destination block is occupied fool!");
            }
        }
        else
        {
            Debug.Log(gameObject.name + " requesting a move to an off-grid destination.");
            RemoveObject(gameObject, fromBlock);
        }
    }

    private void PerformMove(GameObject gameObject, GridBlock from, GridBlock to)
    {
        gameObject.transform.position = GridToWorld(to.location);
        to.isOccupied = true;
        to.objectOnBlock = gameObject;
        from.isOccupied = false;
        from.objectOnBlock = null;
    }

    private void RemoveObject(GameObject gameObject, GridBlock last)
    {
        Debug.Log("RemoveObject() called.");
        Debug.Log("Number of hazards prior to removal: " + hazards.Count);
        Debug.Log("Index of out of bounds element: " + hazards.IndexOf(gameObject));
        hazards.Remove(gameObject);
        Debug.Log("Number of hazards following removal: " + hazards.Count);
        Destroy(gameObject);
        last.isOccupied = false;   
    }

    public void UpdateBoard()
    {
        // This Method will need to gather all of the non-player objects on the board
        // and update the board with each of their behavior.

        if (hazards.Count > 0)
        {
            foreach (GameObject hazard in hazards)
            {
                MovePattern move = hazard.GetComponent<MovePattern>();
                Debug.Log(hazard.name + " is moving by " + move.delta);
                RequestMoveRelative(hazard, move.delta);
            }
        }
        else return;
    }
}