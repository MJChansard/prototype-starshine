using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

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
        
        // Iterate through grid columns
        for (int x = 0; x < gridWidth; x++)
        {
            // Iterate through grid rows
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
      

    public GridBlock FindGridBlockContainingObject(GameObject gameObject)
    {
        for (int x = 0; x < levelGrid.GetLength(0); x++)
        {
            for (int y = 0; y < levelGrid.GetLength(1); y++)
            {
                int objectsOnBlock = levelGrid[x, y].objectsOnBlock.Count;
                if (objectsOnBlock > 0)
                {
                    for (int z = 0; z < objectsOnBlock ; z++)
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

        return null;
    }


    public bool CheckIfGridBlockInBounds(Vector2Int gridLocation)
    {
        if (gridLocation.x >= 0 && gridLocation.x < GridWidth && gridLocation.y >= 0 && gridLocation.y < GridHeight) return true;
        else return false;
    }

    public bool CheckIfGridBlockIsOccupied(Vector2Int gridLocation)
    {
        GridBlock block = FindGridBlockByLocation(gridLocation);
        if (block == null) return false;
        return block.IsOccupied;        
    }   

  

    public void AddObjectToGrid(GameObject gameObject, Vector2Int gridPosition)
    {
        GridBlock destination = levelGrid[gridPosition.x, gridPosition.y];
        destination.objectsOnBlock.Add(gameObject);
    }

    public void RemoveObjectFromGrid(GameObject gameObject, Vector2Int gridPosition)
    {
        GridBlock origin = levelGrid[gridPosition.x, gridPosition.y];

        if (origin.objectsOnBlock.Count > 0)
        {
            for (int i = origin.objectsOnBlock.Count - 1; i == 0 ; i--)
            {
                if (gameObject == origin.objectsOnBlock[i])
                {
                    origin.objectsOnBlock.RemoveAt(i);
                }
                return;
            }            
        }
        else { return; }
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
                    if (block.IsOccupied && block.objectsOnBlock != null)
                    {
                        foreach (GameObject occupant in block.objectsOnBlock)
                        {
                            if (occupant != null)
                            {
                                Handles.Label(GridToWorld(block.location), occupant.name);
                            }
                        }
                    }
                }
            }
        }
    }
}
/*
    public void PlaceObject(GameObject gameObject, Vector2Int position)
    {
        GridBlock target = levelGrid[position.x, position.y];
        if (!target.IsOccupied)
        {
            target.objectOnBlock = gameObject;
            target.IsOccupied = true;

            gameObject.transform.position = GridToWorld(position);
        }

        return;
    }
*/

/*
    public void RemoveObject(GameObject gameObject, GridBlock last)
    {
        // Main responsibilities
        //   - Removing from GridBlock.objectsOnGrid
        //   - No optional parameter to handle transform manipulation
        //   - 
        Debug.Log("RemoveObject() called.");

        Destroy(gameObject);
        last.isOccupied = false;   
    }
*/

/*
    public void UpdateGridPosition(GameObject gameObject, Vector2Int from, Vector2Int to)
    {
        GridBlock fromGridBlock = FindGridBlockByLocation(from);
        GridBlock toGridBlock = FindGridBlockByLocation(to);

        toGridBlock.IsOccupied = true;
        toGridBlock.objectOnBlock = gameObject;
        if (fromGridBlock.objectOnBlock == gameObject)
        {
            fromGridBlock.isOccupied = false;
            fromGridBlock.objectOnBlock = null;
        }
    }
*/
