using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    #region Inspector Attributes
    [SerializeField]
    GameObject[] hazardPrefabs;
    #endregion

    #region Private Fields
    private GridManager gm;
    private List<GridBlock> availableSpawns = new List<GridBlock>();
    
    #endregion
    
    void Start()
    {
        gm = GetComponent<GridManager>();

        Debug.Log("gridWidth: " + gm.gridWidth);
        Debug.Log("gridHeight: " + gm.gridHeight);

        UpdateSpawnLocations();
        PrepareHazard();

        //GameObject asteroid = Instantiate(hazardPrefabs[0]);
        //gm.PlaceObject(asteroid, new Vector2Int(6, 6));
        //gm.hazards.Add(asteroid);
    }

    void Update()
    {
        
    }

    private void UpdateSpawnLocations()
    { 
        // Generate List of available hazard spawn locations
        for (int x = 0; x < gm.gridWidth; x ++)
        {
            for (int y = 0; y < gm.gridHeight; y++)
            {
                if (!gm.levelGrid[x, y].isOccupied && gm.levelGrid[x, y].canSpawn == true)
                {
                    availableSpawns.Add(gm.levelGrid[x, y]);
                }
            }
        }
    }

    private void PrepareHazard()
    {
        int locationIndex = Random.Range(0, availableSpawns.Count);
        int hazardType = Random.Range(0, hazardPrefabs.Length);

        GameObject spawn = Instantiate(hazardPrefabs[hazardType]);
        MovePattern spawnMovement = spawn.GetComponent<MovePattern>();
        
        Vector2Int position = availableSpawns[locationIndex].location;

        int randomlyMove = Random.Range(1, 10);
        // Handle hazards spawning on left side of grid
        if (position.x == 0)
        {
            if (position.y > 0 && position.y < gm.gridHeight)
            {
                spawnMovement.SetMovePatternRight(spawnMovement.moveRate);
            }
            else if (position.y == 0)
            {
                if (randomlyMove < 5)
                {
                    spawnMovement.SetMovePatternRight(spawnMovement.moveRate);
                }
                else
                {
                    spawnMovement.SetMovePatternUp(spawnMovement.moveRate);
                }
            }
            else
            {               
                if (randomlyMove < 5)
                {
                    spawnMovement.SetMovePatternRight(spawnMovement.moveRate);
                }
                else
                {
                    spawnMovement.SetMovePatternDown(spawnMovement.moveRate);
                }
            }
        }

        // Handle hazards spawning on right side of grid
        if (position.x == gm.gridWidth - 1)
        {
            if (position.y > 0 && position.y < gm.gridHeight - 1)
            {
                spawnMovement.SetMovePatternLeft(spawnMovement.moveRate);
            }
            else if (position.y == 0)
            {
                if (randomlyMove < 5)
                {
                    spawnMovement.SetMovePatternLeft(spawnMovement.moveRate);
                }
                else
                {
                    spawnMovement.SetMovePatternUp(spawnMovement.moveRate);
                }
            }
            else
            {
                if (randomlyMove < 5)
                {
                    spawnMovement.SetMovePatternLeft(spawnMovement.moveRate);
                }
                else
                {
                    spawnMovement.SetMovePatternDown(spawnMovement.moveRate);
                }
            }
        }

        // Handle hazard spawning on bottom of grid
        if (position.y == 0 && position.x < gm.gridWidth - 1)
        { 
            spawnMovement.SetMovePatternUp(spawnMovement.moveRate);
        }
        
        if (position.y == gm.gridHeight - 1)
        {
            spawnMovement.SetMovePatternDown(spawnMovement.moveRate);
        }
            
        gm.PlaceObject(spawn, position);
        gm.hazards.Add(spawn);

        // TODO
        // All spawned hazard currently progress down the grid 
        // I need to update MovePattern based on the starting GridBlock
    }

}