using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region Public Attributes

    public GridBlock[,] levelGrid;
    //public bool 

    #endregion


    #region Inspector Attributes
    [SerializeField]
    private int gridColumns = 10;

    [SerializeField]
    private int gridRows = 8;

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
        levelGrid = new GridBlock[gridRows, gridColumns];
        Debug.Log("Object: [levelGrid] created.");
        Debug.Log(levelGrid.Length);

        // Iterate through columns.
        for (int x = 0; x < gridRows; x++)
        {
            // Iterate through rows.
            for (int y = 0; y < gridColumns; y++)
            {
                // Instantiate a GridBlock at each index in the 2D array
                levelGrid[x, y] = new GridBlock(x, y);

                // Add Vector2Int objects
                levelGrid[x, y].location = new Vector2Int(x, y);
            }
        }

        Debug.Log("Object: [levelGrid] successfully created.");
    }


    private void InitializeGrid(GameObject gridPoint, float offset)
    {
        levelGrid = new GridBlock[gridRows, gridColumns];
        Debug.Log("Object: [levelGrid] created.");
        Debug.Log(levelGrid.Length);

        // Iterate through columns.
        for (int x = 0; x < gridRows; x++)
        {
            // Iterate through rows.
            for (int y = 0; y < gridColumns; y++)
            {
                // Instantiate a GridBlock at each index in the 2D array
                levelGrid[x, y] = new GridBlock(x, y);

                // Add Vector2Int objects
                levelGrid[x, y].location = new Vector2Int(x, y);

                // Create Debug grid.  Note y and x are inversed in the Vector3 intentionally
                Instantiate
                (
                    gridPoint,
                    new Vector3(y + offset, x + offset, 0f),
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
        GridBlock fromBlock = levelGrid[from.x, from.y];
        GridBlock toBlock = levelGrid[to.x, to.y];

        if (!toBlock.isOccupied)
        {
            PerformMove(gameObject, fromBlock, toBlock);
        }
        else
        {
            Debug.Log("Destination block is occupied fool!");
        }
    }


    private void PerformMove(GameObject gameObject, GridBlock from, GridBlock to)
    {
        // Convert [to] to a Vector3 and store as [destination]
        // Move [gameObject] to [destination]
        // Update [to.isOccupied] = true
        // update [from.IsOccupied] = false

        gameObject.transform.position = GridToWorld(to.location);
        to.isOccupied = true;
        to.objectOnBlock = gameObject;
        from.isOccupied = false;
        from.objectOnBlock = null;
    }

    public void updateBoard()
    {
        // This Method will need to gather all of the non-player objects on the board
        // and update the board with each of their behavior.

    }

}