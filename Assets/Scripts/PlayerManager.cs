using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements;
using UnityEditorInternal;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Inspector Attributes   
    [SerializeField] private float speed = 2.0f;

    [Header("Player Components")]
    [SerializeField] private GameObject Thruster;
    [SerializeField] private GameObject Cannon;
    [SerializeField] private Transform weaponSource;

    [Header("Weapon Inventory")]
    [SerializeField] private Weapon[] weaponInventory;
    Weapon currentWeapon;
    #endregion

    #region Private Fields
    private GridManager gm;

    private string currentlyFacing = "";
    private Vector2Int delta = Vector2Int.zero;
    private bool isRequestingAttack = false;

    private bool PlayerTurnActive = false;
    //private bool isRequestingMove = false;

    private float attackWaitTime = 2.0f;
    private float moveWaitTime = 1.0f;
    private float waitWaitTime = 0.5f;
    #endregion

    #region Movement Animation Properties
    public Vector3 currentWorldLocation;
    public Vector3 targetWorldLocation;
    
    private float moveSpeed = 1.0f;
    #endregion

    #region Coroutine Status
    private bool thrusterCoroutineIsRunning = false;
    private bool cannonCoroutineIsRunning = false;
    private bool moveCoroutineIsRunning = false;
    #endregion

    public System.Action OnPlayerAdvance;
    public System.Action<Hazard, Vector2Int> OnPlayerAddHazard;

    private void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GridManager>();
        currentWeapon = weaponInventory[0];
    }

    void Update()
    {
        PlayerInput();
        
        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Currently facing: " + currentlyFacing);
        }
    }


    void PlayerInput()
    {
        // Weapon Selection
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectWeapon(-1);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SelectWeapon(1);
        }

        // Player Movement & turn advancement
        if (thrusterCoroutineIsRunning == false && moveCoroutineIsRunning == false && cannonCoroutineIsRunning == false)
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

            if (Input.GetKeyDown(KeyCode.F))
            {
                isRequestingAttack = true;
                if (OnPlayerAdvance != null) OnPlayerAdvance();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (OnPlayerAdvance != null) OnPlayerAdvance();
            }
        }
        else
        {
            return;
        }
    }


    private void SelectWeapon(int choice)
    {
        int weaponIndex = Array.IndexOf(weaponInventory, currentWeapon);
        if (choice == -1 && weaponIndex > 0)
        {
            currentWeapon = weaponInventory[weaponIndex - 1];
            Debug.LogFormat("Weapon swap completed.  Current Weapon: {0}", currentWeapon.GetComponent<Weapon>().Name);
        }

        if (choice == 1 && weaponIndex < weaponInventory.Length - 1)
        {            
            currentWeapon = weaponInventory[weaponIndex + 1];
            Debug.LogFormat("Weapon swap completed.  Current Weapon: {0}", currentWeapon.GetComponent<Weapon>().Name);
        }
    }

   
    public float OnTickUpdate()
    {
        if (isRequestingAttack)
        {
            isRequestingAttack = false;
            Attack(currentWeapon);
            return attackWaitTime;
        }
        else if (delta != Vector2Int.zero)
        {
            Move();
            return moveWaitTime;
        }
        
        return waitWaitTime;
    }


    private void Move()
    {
        GridBlock current = gm.FindGridBlockContainingObject(this.gameObject);
        bool canMove = gm.CheckIfMoveIsValid(this.gameObject, current.location, current.location + delta);

        if (canMove)
        {
            targetWorldLocation = gm.GridToWorld(current.location + delta);
            
            if (thrusterCoroutineIsRunning == false) StartCoroutine(AnimateThrusterCoroutine());
            if (moveCoroutineIsRunning == false) StartCoroutine(AnimateMovementCoroutine());   
        }

    }

    private IEnumerator AnimateThrusterCoroutine()
    {
        thrusterCoroutineIsRunning = true;

        ParticleSystem ps = Thruster.GetComponent<ParticleSystem>();
        ps.Play();
        yield return new WaitForSeconds(1.0f);
        ps.Stop();

        thrusterCoroutineIsRunning = false;
    }

    private IEnumerator AnimateMovementCoroutine()
    {
        moveCoroutineIsRunning = true;

        // D = s * t
        float distance = Vector3.Distance(currentWorldLocation, targetWorldLocation);
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= 1.0f)
        {
            float traveled = (Time.time - startTime) * moveSpeed;
            percentTraveled = traveled / distance;  // Interpolator for Vector3.Lerp
            transform.position = Vector3.Lerp(currentWorldLocation, targetWorldLocation, Mathf.SmoothStep(0f, 1f, percentTraveled));

            yield return null;
        }
        
        currentWorldLocation = targetWorldLocation;
        moveCoroutineIsRunning = false;
    }


    private void Attack(Weapon withWeapon)
    {
        /*  STEPS
         *   - Need to find current location on the grid (GridBlock)
         *   - Determine which direction player is facing
         *   - Gather all GridBlocks on the cannon trajectory
         *   - Test each block for a destructible GameObject
         *   - Apply damage to the first detected GameObject
         *   - Update Tick
         */

        GridBlock currentGrid = gm.FindGridBlockContainingObject(this.gameObject);

        if (currentWeapon.GetComponent<Weapon>().Name == "Missile Launcher")
        {
            GridBlock targetGrid = gm.FindGridBlockByLocation(currentGrid.location + delta);

            MissileLauncher launcher = currentWeapon.GetComponent<MissileLauncher>();
            //launcher.LaunchMissile(currentGrid, targetGrid);
            StartCoroutine(launcher.LaunchMissileCoroutine(currentGrid, targetGrid));

            return;
        }

        // Determine weapon path
        List<GridBlock> possibleTargets = new List<GridBlock>();
        switch (currentlyFacing)
        {
            // Facing forward
            case "Up":
                for (int y = currentGrid.location.y + 1; y < gm.GridHeight; y++)
                {
                    possibleTargets.Add(gm.levelGrid[currentGrid.location.x, y]);
                }
                break;
            // Facing down
            case "Down":
                for (int y = currentGrid.location.y - 1; y >= 0; y--)
                {
                    possibleTargets.Add(gm.levelGrid[currentGrid.location.x, y]);
                }
                break;
            // Facing left
            case "Left":
                for (int x = currentGrid.location.x -1 ; x >= 0; x--)
                {
                    possibleTargets.Add(gm.levelGrid[x, currentGrid.location.y]);
                }
                break;
            // Facing right
            case "Right":
                for (int x = currentGrid.location.x + 1; x < gm.GridWidth; x++)
                {
                    possibleTargets.Add(gm.levelGrid[x, currentGrid.location.y]);
                }
                break;
        }

        foreach (GridBlock target in possibleTargets)
        {
            if (target.isOccupied == true)
            {
                Health hp = target.objectOnBlock.GetComponent<Health>();

                if (hp != null)
                {
                    withWeapon.StartAnimationCoroutine(target);
                    hp.ApplyDamage(withWeapon.Damage);
                    Debug.Log("Target's current health: " + hp.CurrentHP);
                }

                break;
            }
        }
    }


}
