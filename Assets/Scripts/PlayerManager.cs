using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements;
using UnityEditorInternal;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public bool InputActive = true;

    public Vector3 ReportDirection
    {
        get { return new Vector3(currentlyFacing.x, currentlyFacing.y, 0.0f);  }
    }

    #region Inspector Attributes   
    [SerializeField] private float speed = 2.0f;

    [Header("Player Components")]
    [SerializeField] private GameObject Thruster;
    [SerializeField] private GameObject Cannon;
    [SerializeField] private Transform weaponSource;

    [Header("Weapon Inventory")]
    [SerializeField] private Weapon[] weaponInventory;
    Weapon currentWeapon;
    [SerializeField] private Sprite[] weaponIconsUI;
    #endregion

    #region References
    private GridManager gm;
    private UiPlayerDisplay ui;
    #endregion

    #region Private Fields
    private Vector2Int currentlyFacing = Vector2Int.up;
    private Vector2Int delta = Vector2Int.zero;
    private bool isRequestingAttack = false;   

    private float attackWaitTime = 2.0f;
    private float moveWaitTime = 1.0f;
    private float waitWaitTime = 0.5f;

    private int amountOfJumpFuelStored = 0;
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
    public System.Action<Hazard, Vector2Int, bool> OnPlayerAddHazard;

    private void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GridManager>();
        currentWeapon = weaponInventory[0];
        ui = FindObjectOfType<UiPlayerDisplay>();
        ui.SetDisplayWeapon(weaponIconsUI[0]);
    }

    void Update()
    {
        if (InputActive) PlayerInput();
        
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
                currentlyFacing = delta = Vector2Int.up;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
                currentlyFacing = delta = Vector2Int.down;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.forward);
                currentlyFacing = delta = Vector2Int.left;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.forward);
                currentlyFacing = delta = Vector2Int.right;
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
        int currentWeaponIndex = Array.IndexOf(weaponInventory, currentWeapon);
        int newWeaponIndex = currentWeaponIndex += choice;

        if (newWeaponIndex >= 0 && newWeaponIndex < weaponInventory.Length)
        {
            currentWeapon = weaponInventory[newWeaponIndex];
            //ui.SetDisplayWeapon(weaponIconsUI[newWeaponIndex]);
            ui.SetDisplayWeapon(currentWeapon);

        }
    }

   
    public float OnTickUpdate()
    {
        if (isRequestingAttack)
        {
            isRequestingAttack = false;
            Attack();
            return attackWaitTime;
        }
        else if (delta != Vector2Int.zero)
        {
            Vector2Int destinationGrid = Move();
            GatherLoot(destinationGrid);
            return moveWaitTime;
            
        }
        
        return waitWaitTime;
    }


    private Vector2Int Move()
    {
        Vector2Int originGridPosition = gm.FindGridBlockContainingObject(this.gameObject).location;
        if (originGridPosition == null) Debug.LogError("Unable to find Player object on the Grid.");

        Vector2Int destinationGridPosition = originGridPosition + delta;

        bool moveIsValid = gm.CheckIfGridBlockInBounds(originGridPosition) && !gm.CheckIfGridBlockIsOccupied(destinationGridPosition);
        Debug.Log("Player move is validated: " + moveIsValid);

        if (moveIsValid)
        {
            targetWorldLocation = gm.GridToWorld(destinationGridPosition);

            if (thrusterCoroutineIsRunning == false) StartCoroutine(AnimateThrusterCoroutine());
            if (moveCoroutineIsRunning == false) StartCoroutine(AnimateMovementCoroutine());

            gm.AddObjectToGrid(this.gameObject, destinationGridPosition);
            gm.RemoveObjectFromGrid(this.gameObject, originGridPosition);

            return destinationGridPosition;
        }
        else return originGridPosition;
    }

    private void GatherLoot(Vector2Int gridBlockWithLoot)
    {
        // Gather loot if any exists
        Debug.Log("PlayerManager.GatherLoot() called.");
        List<GameObject> collectedLoot = new List<GameObject>();

        //Vector2Int currentGrid = gm.WorldToGrid(currentWorldLocation);
        GridBlock lootBlock = gm.FindGridBlockByLocation(gridBlockWithLoot);
        
        for (int i = lootBlock.lootOnBlock.Count - 1; i >= 0; i--)
        {
            LootData currentLoot = lootBlock.lootOnBlock[i];
            if (currentLoot.LootItemName == "Jump Fuel")
            {
                Debug.Log("Jump Fuel found.");
                amountOfJumpFuelStored += currentLoot.JumpFuelIncrement;
                collectedLoot.Add(currentLoot.gameObject);
                gm.RemoveLootFromGrid(currentLoot, lootBlock.location);
            }
        }

        for (int i = collectedLoot.Count - 1; i >= 0; i--)
        {
            Debug.Log("Destroying collected loot.");
            Destroy(collectedLoot[i], moveWaitTime);
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


    private void Attack()
    {

        GridBlock currentGridBlock = gm.FindGridBlockContainingObject(this.gameObject);
        Vector2Int currentGridLocation = currentGridBlock.location;

        if (currentWeapon.GetComponent<Weapon>().Name == "Missile Launcher")
        {
            MissileLauncher launcher = currentWeapon.GetComponent<MissileLauncher>();
            Hazard launchedMissile = launcher.LaunchMissile(currentGridBlock, currentlyFacing);

            if (OnPlayerAddHazard != null)
            {
                Debug.Log("PlayerManager.OnPlayerAddHazard() called.");
                OnPlayerAddHazard(launchedMissile, currentGridBlock.location, false);
            }
            else
            {
                Debug.LogError("No subscribers to OnPlayerHazard().");
            }

            return;
        }


        // Identify possible targets based on PlayerManager.currentlyFacing
        // Targets eligible for both AutoCannon and Railgun
        List<GridBlock> possibleTargetBlocks = new List<GridBlock>();
        if (currentlyFacing == Vector2Int.up)
        {
            for (int y = currentGridLocation.y + 1; y < gm.GridHeight; y++)
            {
                if (gm.levelGrid[currentGridLocation.x, y].IsOccupied)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[currentGridLocation.x, y]);
                }
            }
        }
        else if (currentlyFacing == Vector2Int.down)
        {
            for (int y = currentGridLocation.y - 1; y >= 0; y--)
            {
                if (gm.levelGrid[currentGridLocation.x, y].IsOccupied)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[currentGridLocation.x, y]);
                }
            }
        }
        else if (currentlyFacing == Vector2Int.left)
        {
            for (int x = currentGridLocation.x - 1; x >= 0; x--)
            {
                if (gm.levelGrid[x, currentGridLocation.y].IsOccupied)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[x, currentGridLocation.y]);
                }
            }
        }
        else if (currentlyFacing == Vector2Int.right)
        {
            for (int x = currentGridLocation.x + 1; x < gm.GridWidth; x++)
            {
                if (gm.levelGrid[x, currentGridLocation.y].IsOccupied)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[x, currentGridLocation.y]);
                }
            }
        }


        if (currentWeapon.GetComponent<Weapon>().Name == "AutoCannon")
        {
            for (int i = 0; i < possibleTargetBlocks.Count; i++)
            {
                for (int j = 0; j < possibleTargetBlocks[i].objectsOnBlock.Count; j++)
                {
                    Health hp = possibleTargetBlocks[i].objectsOnBlock[j].GetComponent<Health>();
                    if (hp != null)
                    {
                        currentWeapon.StartAnimationCoroutine(possibleTargetBlocks[i]);
                        hp.SubtractHealth(currentWeapon.Damage);
                        Debug.Log("Target's current health: " + hp.CurrentHP);

                        return;
                    }
                }
            }
        }

        if (currentWeapon.Name == "Railgun")
        {
            /*  STEPS
             *   - Instantiate the Rail at Player's current location
             *   - Start the animation coroutine, which propels the rail from the Player's location, in the
             *      direction the player is facing, to the end of the grid
             *   - Apply damage to all objects in between the Player and the end of the grid
             *   - Move the player back two GridBlocks
             */

            RailGun railGun = currentWeapon.GetComponent<RailGun>();
            railGun.FireRailgun(currentWorldLocation);

            // Determine destination/final GridBlock
            GridBlock endAnimationGridLocation;
            if (currentlyFacing == Vector2Int.up)
            {
                Vector2Int index = new Vector2Int(currentGridLocation.x, gm.GridHeight - 1);
                Debug.LogFormat("Index when facing up: {0}", index);
                endAnimationGridLocation = gm.FindGridBlockByLocation(index);
            }
            else if(currentlyFacing == Vector2Int.down)
            {
                Vector2Int index = new Vector2Int(currentGridLocation.x, 0);
                Debug.LogFormat("Index when facing down: {0}", index);
                endAnimationGridLocation = gm.FindGridBlockByLocation(index);
            }
            else if(currentlyFacing == Vector2Int.left)
            {
                Vector2Int index = new Vector2Int(0, currentGridLocation.y);
                Debug.LogFormat("Index when facing left: {0}", index);
                endAnimationGridLocation = gm.FindGridBlockByLocation(index);
            }
            else
            {
                Vector2Int index = new Vector2Int(gm.GridWidth - 1, currentGridLocation.y);
                Debug.LogFormat("Index when facing right: {0}", index);
                endAnimationGridLocation = gm.FindGridBlockByLocation(index);
            }
            
            railGun.StartAnimationCoroutine(endAnimationGridLocation);

            // Apply damage
            for (int i = 0; i < possibleTargetBlocks.Count; i++)
            {
                for (int j = 0; j < possibleTargetBlocks[i].objectsOnBlock.Count; j++)
                {
                    Health hp = possibleTargetBlocks[i].objectsOnBlock[j].GetComponent<Health>();
                    if (hp != null)
                    {
                        hp.SubtractHealth(currentWeapon.Damage);
                    }
                }
            }
        }   
    }
}
