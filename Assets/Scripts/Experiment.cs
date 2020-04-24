using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment : MonoBehaviour
{
    #region Public Attributes

    public GridBlock[,] levelGrid;

    #endregion


    #region Inspector Attributes
    [SerializeField]
    private int gridColumns = 10;

    [SerializeField]
    private int gridRows = 8;

    [SerializeField]
    GameObject playerPrefab;
    #endregion


    #region Private Attributes
    GameObject player;
    PlayerManager playerManager;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();

        Instantiate
        (
            playerPrefab,
            levelGrid[3, 4].location,
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

    /*    
        void InitializeGrid()
        {
            levelGrid = new Vector3[gridRows, gridColumns];                 // Multidimensional arrays parameter order: rows, columns

            // Iterate through columns.
            for (int x = 0; x < gridRows; x++)
            {
                // Iterate through rows.
                for (int y = 0; y < gridColumns; y++)
                {
                    // Add Vector3 objects
                    levelGrid[x, y] = new Vector3(x, y, 0f);
                }
            }

            Debug.Log("Object: [levelGrid] successfully created.");
        }
    */

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
                levelGrid[x, y] = new GridBlock(x, y, 0);

                // Add Vector3 objects
                levelGrid[x, y].location = new Vector3(x, y, 0f);
            }
        }

        Debug.Log("Object: [levelGrid] successfully created.");

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

    public void RequestMove(GameObject gameObject, Vector3 source, Vector3 destination)
    {
        // Locate GridBlock for destination parameter
        // Check GridBlock.isOccupied property
        // If not occupied, call PerformMove()

        GridBlock fromBlock = levelGrid[(int)source.x, (int)source.y];
        GridBlock toBlock = levelGrid[(int)destination.x, (int)destination.y];
        if (!toBlock.isOccupied)
        {
            PerformMove(gameObject, fromBlock, toBlock);
        }

        return;
    }

    private void PerformMove(GameObject gameObject, GridBlock from, GridBlock to)
    {
        // Convert [to] to a Vector3 and store as [destination]
        // Move [gameObject] to [destination]
        // Update [to.isOccupied] = true
        // update [from.IsOccupied] = false

        gameObject.transform.position = new Vector3(to.location.x, to.location.y, to.location.z);
        to.isOccupied = true;
        to.objectOnBlock = gameObject;
        from.isOccupied = false;
        from.objectOnBlock = null;
    }
}
