using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Inspector Attributes

    [SerializeField]
    private float speed = 2.0f;
    [SerializeField]
    private Transform weaponSource;
    [SerializeField]
    private GameObject missile;

    #endregion

    #region Private Fields
    private GridManager gm;

    private string currentlyFacing = "";
    private Vector2Int delta = Vector2Int.zero;

    #endregion


    public System.Action OnPlayerAdvance;

    private void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GridManager>();
    }

    void Update()
    {
        Movement();
        FireCannon();

        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Currently facing: " + currentlyFacing);
        }
    }

    void Movement()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {

            transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            currentlyFacing = "Up";
            delta = Vector2Int.up;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
            currentlyFacing = "Down";
            delta = Vector2Int.down;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.forward);
            currentlyFacing = "Left";
            delta = Vector2Int.left;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.forward);
            currentlyFacing = "Right";
            delta = Vector2Int.right;
        }
               
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (OnPlayerAdvance != null)
            {
                OnPlayerAdvance();
            }
        }
    }

    public void MovePlayer()
    {
        GridBlock current = gm.FindGridBlockContainingObject(this.gameObject);
        gm.RequestMove(this.gameObject, current.location, current.location + delta);
    }

    void FireCannon()
    {
       
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindCannonTarget();
        }
    }

    private GridBlock FindCannonTarget()
    {
        /*  STEPS
             *   - Need to find current location on the grid (GridBlock)
             *   - Determine which direction player is facing
             *   - Gather all GridBlocks on the cannon trajectory
             *   - Test each block for a destructible GameObject
             *   - Apply damage to the first detected GameObject
             *   - Update Tick
             */

        GridBlock currentlyAt = gm.FindGridBlockContainingObject(this.gameObject);
        
        // Determine weapon path
        List<GridBlock> possibleTargets = new List<GridBlock>();
        switch (currentlyFacing)
        {
            // Facing forward
            case "Up":
                for (int y = currentlyAt.location.y + 1; y < gm.GridHeight; y++)
                {
                    possibleTargets.Add(gm.levelGrid[currentlyAt.location.x, y]);
                }
                break;
            // Facing down
            case "Down":
                for (int y = currentlyAt.location.y - 1; y >= 0; y--)
                {
                    possibleTargets.Add(gm.levelGrid[currentlyAt.location.x, y]);
                }
                break;
            // Facing left
            case "Left":
                for (int x = currentlyAt.location.x -1 ; x >= 0; x--)
                {
                    possibleTargets.Add(gm.levelGrid[x, currentlyAt.location.y]);
                }
                break;
            // Facing right
            case "Right":
                for (int x = currentlyAt.location.x + 1; x > gm.GridWidth; x++)
                {
                    possibleTargets.Add(gm.levelGrid[x, currentlyAt.location.y]);
                }
                break;
        }

        foreach (GridBlock target in possibleTargets)
        {
            if (target.isOccupied == true)
            {
                Debug.Log("Cannon target located:" + target.objectOnBlock);
                return target;       
            }
        }

        return null;
    }

}
