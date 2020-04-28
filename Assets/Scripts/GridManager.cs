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

        Instantiate
        (
            playerPrefab,
            //levelGrid[3, 4].location,
            GridToWorld(new Vector2Int(5, 4)),
            Quaternion.identity
        );

        player = GameObject.FindGameObjectWithTag("Player");
        playerManager = player.GetComponent<PlayerManager>();

        AssignObject
        (
            levelGrid
            [
                playerManager.playerX,
                playerManager.playerY
            ],
            player
        );

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


    private void AssignObject(GridBlock gridBlock, GameObject gameObject)
    {
        // The gridBlock parameter needs to reference the GridBlock in levelGrid
        // The gameObject parameter needs to reference the object in the GridBlock
        // IsOccupied parameter needs to be updated

        GridBlock target = levelGrid[(int)gridBlock.location.x, (int)gridBlock.location.y];
        if (!target.isOccupied)
        {
            target.isOccupied = true;
            target.objectOnBlock = gameObject;
        }

        return;
    }

    /*  Update to 
     * 
     * 
     * 
     */
    public void RequestMove(GameObject gameObject, Vector3 destination)
    {
        int currentGridRow = WorldToGrid(gameObject.transform.position).x;
        int currentGridCol = WorldToGrid(gameObject.transform.position).y;
        GridBlock fromBlock = levelGrid[currentGridRow, currentGridCol];

        int targetGridRow = WorldToGrid(destination).x;
        int targetGridCol = WorldToGrid(destination).y;
        GridBlock toBlock = levelGrid[targetGridRow, targetGridCol];
        
        if (!toBlock.isOccupied)
        {
            PerformMove(gameObject, fromBlock, toBlock);
        }
    }


    private void PerformMove(GameObject gameObject, GridBlock from, GridBlock to)
    {
        // Convert [to] to a Vector3 and store as [destination]
        // Move [gameObject] to [destination]
        // Update [to.isOccupied] = true
        // update [from.IsOccupied] = false

        gameObject.transform.position = new Vector3(to.location.x, to.location.y, 0f);
        to.isOccupied = true;
        to.objectOnBlock = gameObject;
        from.isOccupied = false;
        from.objectOnBlock = null;
    }
}