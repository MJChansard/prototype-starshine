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

    #region Player Attributes   
    [SerializeField] private float speed = 2.0f;

    [Header("Player Components")]
    [SerializeField] private GameObject Thruster;
    [SerializeField] private GameObject Cannon;
    [SerializeField] private Transform weaponSource;

    [Header("Weapon Inventory")]
    [SerializeField] private Weapon[] weaponInventory;
    int selectedWeaponIndex;
    #endregion

    #region References
    private GridManager gm;
    private PlayerHUD ui;
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

    private void Awake()
    {
        selectedWeaponIndex = 0;
    }

    private void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GridManager>();
        
        ui = FindObjectOfType<PlayerHUD>();
        ui.Init(weaponInventory);
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
        int newWeaponIndex = selectedWeaponIndex + choice;
        Debug.LogFormat("Index of current weapon: {0}, Index of new weapon: {1}", selectedWeaponIndex, newWeaponIndex);
        if (newWeaponIndex >= 0 && newWeaponIndex < weaponInventory.Length)
        {
            ui.UpdateWeaponSelection(selectedWeaponIndex, newWeaponIndex);
            selectedWeaponIndex = newWeaponIndex;
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

        bool moveIsValid = gm.CheckIfGridBlockInBounds(originGridPosition) && gm.CheckIfGridBlockIsAvailable(destinationGridPosition);
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
        
        GridBlock lootBlock = gm.FindGridBlockByLocation(gridBlockWithLoot);

        for (int i = lootBlock.objectsOnBlock.Count - 1; i >= 0; i--)
        {
            LootData currentLoot = lootBlock.objectsOnBlock[i].GetComponent<LootData>();
            if (currentLoot != null)
            {
                if (currentLoot.Type == LootData.LootType.JumpFuel)
                {
                    Debug.Log("Jump Fuel found.");
                    amountOfJumpFuelStored += currentLoot.LootAmount;
                    ui.UpdateHUDFuel(amountOfJumpFuelStored);
                    gm.RemoveObjectFromGrid(currentLoot.gameObject, lootBlock.location);
                    Destroy(currentLoot.gameObject, moveWaitTime);
                }
            }
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
        Weapon currentWeapon = weaponInventory[selectedWeaponIndex];

        if (currentWeapon.Name == "Missile Launcher")
        {
            MissileLauncher launcher = currentWeapon.GetComponent<MissileLauncher>();
            Hazard launchedMissile = launcher.LaunchMissile(currentGridBlock, currentlyFacing);
            /*
            if (launchedMissile != null && OnPlayerAddHazard != null)
            {
                Debug.Log("PlayerManager.OnPlayerAddHazard() called.");
                OnPlayerAddHazard(launchedMissile, currentGridBlock.location, false);
            }
            else
            {
                Debug.LogError("No subscribers to OnPlayerHazard().");
            }
            */
            if (launchedMissile == null)
            {
                Debug.Log("Out of Missile Launcher ammo.");
                return;
            }
            else if (OnPlayerAddHazard == null)
            {
                Debug.Log("No subscribers to OnPlayerAddHazard().");
                return;
            }
            else
            {
                OnPlayerAddHazard(launchedMissile, currentGridBlock.location, false);
                ui.UpdateHUDWeapons(selectedWeaponIndex, launcher.weaponAmmunition);
                return;
            }
        }


        // Identify possible targets based on PlayerManager.currentlyFacing
        // Targets eligible for both AutoCannon and Railgun
        List<GridBlock> possibleTargetBlocks = new List<GridBlock>();
        if (currentlyFacing == Vector2Int.up)
        {
            for (int y = currentGridLocation.y + 1; y < gm.GridHeight; y++)
            {
                if (gm.levelGrid[currentGridLocation.x, y].objectsOnBlock.Count > 0)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[currentGridLocation.x, y]);
                }
            }
        }
        else if (currentlyFacing == Vector2Int.down)
        {
            for (int y = currentGridLocation.y - 1; y >= 0; y--)
            {
                if (gm.levelGrid[currentGridLocation.x, y].objectsOnBlock.Count > 0)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[currentGridLocation.x, y]);
                }
            }
        }
        else if (currentlyFacing == Vector2Int.left)
        {
            for (int x = currentGridLocation.x - 1; x >= 0; x--)
            {
                if (gm.levelGrid[x, currentGridLocation.y].objectsOnBlock.Count > 0)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[x, currentGridLocation.y]);
                }
            }
        }
        else if (currentlyFacing == Vector2Int.right)
        {
            for (int x = currentGridLocation.x + 1; x < gm.GridWidth; x++)
            {
                if (gm.levelGrid[x, currentGridLocation.y].objectsOnBlock.Count > 0)
                {
                    possibleTargetBlocks.Add(gm.levelGrid[x, currentGridLocation.y]);
                }
            }
        }


        if (currentWeapon.Name == "AutoCannon")
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
            ui.UpdateHUDWeapons(selectedWeaponIndex, railGun.weaponAmmunition);
                
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
