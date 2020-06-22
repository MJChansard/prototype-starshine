using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region Public Properties
    public int GridWidth
    {
        get { return gridWidth;}
    }
    public int GridHeight
    {
        get { return gridHeight; }
    }
    public Transform gridContainer;
    #endregion

    #region Inspector Attributes    
    [SerializeField]
    private int gridSpacing = 1;

    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 8;

    [SerializeField] private GameObject debugGridPrefab;
    [SerializeField] private bool gridBlockLabels = false;
    #endregion

    public System.Action OnUpdateBoard;
    public GridBlock[,] levelGrid;

    public void Init()
    {
       
        InitializeGrid(debugGridPrefab, 0f);
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
                    gridContainer
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
        GridBlock target = levelGrid[position.x, position.y];  
        if (!target.isOccupied)
        {
            target.objectOnBlock = gameObject;
            target.isOccupied = true;

            gameObject.transform.position = GridToWorld(position);
        }

        return;
    }
    

    public GridBlock FindGridBlockContainingObject(GameObject gameObject)
    {
        for (int x = 0; x < levelGrid.GetLength(0); x++)
        {
            for (int y = 0; y < levelGrid.GetLength(1); y++)
            {
                if (levelGrid[x, y].objectOnBlock == gameObject)
                {
                    return levelGrid[x, y];
                }   
            }
        }

        Debug.LogError("Game Object not found!");
        return null;
    }

    public GridBlock FindGridBlockByLocation(Vector2Int location)
    {
        for (int x = 0; x < levelGrid.GetLength(0); x++)
        {
            for (int y = 0; y < levelGrid.GetLength(1); y++)
            {
                if (levelGrid[x, y].location == location)
                {
                    return levelGrid[x, y];
                }
            }
        }

        Debug.LogError("No GridBlock found for provided location.");
        return null;
    }
/*
    public bool CheckIfMoveIsValid(GameObject gameObject, Vector2Int from, Vector2Int to)
    {        
        GridBlock fromBlock = levelGrid[from.x, from.y];
        GridBlock toBlock;

        // Ensure valid destination
        if (to.x >= 0 && to.x < GridWidth && to.y >= 0 && to.y < GridHeight)
        {
            toBlock = levelGrid[to.x, to.y];

            if (!toBlock.isOccupied)
            {
                // #HERE
                UpdateGridPosition(gameObject, fromBlock.location, toBlock.location);
                return true;
            }
            else
            {
                Debug.Log("Destination block is occupied fool!");
                return false;
            }
        }
        else
        {
            Debug.Log(gameObject.name + " requesting to leave grid.");
            return false;
        }
    }
*/
    public bool CheckIfGridBlockInBounds(Vector2Int gridLocation)
    {
        if (gridLocation.x >= 0 && gridLocation.x < GridWidth && gridLocation.y >= 0 && gridLocation.y < GridHeight) return true;
        else return false;
    }

    public bool CheckIfGridBlockIsUnoccupied(Vector2Int gridLocation)
    {
        GridBlock block = FindGridBlockByLocation(gridLocation);

        if (block.isOccupied == true) return false;
        else return true;
    }


    public void UpdateGridPosition(GameObject gameObject, Vector2Int from, Vector2Int to)
    {
        GridBlock fromGridBlock = FindGridBlockByLocation(from);
        GridBlock toGridBlock = FindGridBlockByLocation(to);

        toGridBlock.isOccupied = true;
        toGridBlock.objectOnBlock = gameObject;
        if(fromGridBlock.objectOnBlock == gameObject)
        {
            fromGridBlock.isOccupied = false;
            fromGridBlock.objectOnBlock = null;
        }
    }

   
    public void RemoveObject(GameObject gameObject, GridBlock last)
    {
        Debug.Log("RemoveObject() called.");
                        
        Destroy(gameObject);
        last.isOccupied = false;   
    }

    private void OnDrawGizmos()
    {
        if (gridBlockLabels == true)
        {
            Gizmos.color = Color.blue;
            foreach (GridBlock block in levelGrid)
            {
                if (block.isOccupied && block.objectOnBlock != null)
                {
                    Handles.Label(GridToWorld(block.location), block.objectOnBlock.name);
                }
            }
        }
    }
}