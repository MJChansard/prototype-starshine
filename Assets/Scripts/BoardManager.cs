using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    #region Public Attributes

    public int columns = 10;
    public int rows = 8;

    #endregion

    #region Inspector Attributes

    public GameObject exit;
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerWallTiles;

    private Transform boardHolder;

    private List<Vector3> gridPositions = new List<Vector3>();      // A list of possible locations to place tiles

    #endregion


    #region Methods

    void InitializeList()
    {
        //Clear List gridPositions.
        gridPositions.Clear();

        //Loop through x axis (columns).
        for (int x = 0; x < columns; x++)
        {
            //Within each column, loop through y axis (rows)
            for (int y = 0; y < rows; y++)
            {
                //At each index add a new Vector3 to our List with the x and y coordinates of that position
                gridPositions.Add(new Vector3(x, y, 0.0f));
            }
        }
    }

    #endregion
}
