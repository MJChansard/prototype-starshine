using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Player : GridObject
{
    [Header("Player Components")]
    [SerializeField] private GameObject Thruster;
    
    [Header("Weapon Inventory")]
    [SerializeField] private GameObject[] weaponObjects;
    [SerializeField] private Transform weaponSource;
        
    private Weapon[] weaponInventory;
    private int indexSelectedWeapon = 0;

    //[SerializeField] private float speed = 2.0f;
    
    [Title("BUTTONS")]
    [Button]
    private void InflictDamage()
    {
        hp.SubtractHealth(10);
        //ui.UpdateHpHUD(hp.CurrentHP, 40);
    }

    // Tick update fields
    public bool InputActive = true;
    public bool IsAttackingThisTick = false;

    public Vector2Int Direction { get { return currentlyFacing; } }
    private Vector2Int currentlyFacing;
    
    private float attackWaitTime = 2.0f;
    private float moveWaitTime = 1.0f;
    private float waitWaitTime = 0.0f;

    private PlayerHUD ui;
    private MovePattern movePattern;
    private Health hp;

    public int CurrentJumpFuel { get { return currentJumpFuel; } }
    private int currentJumpFuel = 0;
    private int maxFuelAmount = 10;

    //public GameObject SelectedWeaponProjectile { get { return weaponInventory[indexSelectedWeapon].WeaponPrefab; } }

    public Weapon SelectedWeapon { get { return weaponInventory[indexSelectedWeapon]; } }
    public bool IsAlive { get { return hp.HasHP; } }
    

    public System.Action OnPlayerAdvance;
    //public System.Action<GridObject, Vector2Int, bool> OnPlayerAddHazard;

    public System.Action<int, int, float> OnAmmoAmountChange;
    public System.Action<int, int, float> OnFuelAmountChange;


    private void Start()
    {
        movePattern = GetComponent<MovePattern>();
        movePattern.SetMovePatternUp();

        hp = GetComponent<Health>();

        weaponInventory = new Weapon[weaponObjects.Length];
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            Weapon toInsert = weaponObjects[i].GetComponent<Weapon>();
            weaponInventory[i] = toInsert;
            Debug.LogFormat("Added {0} to weaponInventory.", toInsert.Name);
        }

        ui = FindObjectOfType<PlayerHUD>();
        //ui.Init(weaponInventory, maxFuelAmount, hp.CurrentHP);

        FindObjectOfType<PlayerHUD>().Init(weaponInventory, maxFuelAmount, hp.CurrentHP);

        //HideProperty = true;
    }
    private void Update()
    {
        if (InputActive) PlayerInput();

        if (Input.GetKeyDown(KeyCode.T)) Debug.LogFormat("Currently facing: {0}", currentlyFacing);
    }


    private void PlayerInput()
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
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            currentlyFacing = Vector2Int.up;
            movePattern.SetMovePatternUp();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
            currentlyFacing = Vector2Int.down;
            movePattern.SetMovePatternDown();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.forward);
            currentlyFacing = Vector2Int.left;
            movePattern.SetMovePatternLeft();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.forward);
            currentlyFacing = Vector2Int.right;
            movePattern.SetMovePatternRight();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            Weapon w = weaponInventory[indexSelectedWeapon];

            if (w.currentAmmunition > 0)
            {
                IsAttackingThisTick = true;
                w.SubtractAmmo();                
                OnAmmoAmountChange?.Invoke(indexSelectedWeapon, w.currentAmmunition, 0.0f);
            }
            
            if (OnPlayerAdvance != null) OnPlayerAdvance();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (OnPlayerAdvance != null) OnPlayerAdvance();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            // Debug button
        }
    }


    private void SelectWeapon(int choice)
    {
        int newWeaponIndex = indexSelectedWeapon + choice;
        Debug.LogFormat("Index of current weapon: {0}, Index of new weapon: {1}", indexSelectedWeapon, newWeaponIndex);
        if (newWeaponIndex >= 0 && newWeaponIndex < weaponInventory.Length)
        {
            ui.UpdateWeaponSelection(indexSelectedWeapon, newWeaponIndex);
            indexSelectedWeapon = newWeaponIndex;
        }
    }
    public void ExecuteAttackAnimation(GridBlock gridBlock)
    {
        weaponInventory[indexSelectedWeapon].StartAnimationCoroutine(gridBlock);
    }
/*
    public void AcceptLoot(Loot.LootType type, int amount)
    {
        if (type == Loot.LootType.JumpFuel)
        {
            Debug.Log("Jump Fuel found.");
            currentJumpFuel += amount;
        }

        if (type == Loot.LootType.MissileAmmo)
        {
            Debug.Log("Missile Ammo found.");
            for (int j = 0; j < weaponInventory.Length; j++)
            {
                if (weaponInventory[j].Name == "Missile Launcher")
                {
                    weaponInventory[j].weaponAmmunition += amount;
                    ui.UpdateHUDWeapons(j, weaponInventory[j].weaponAmmunition);
                    break;
                }
            }
        }
    }
*/
    public void AcceptAmmo(WeaponType type, int amount)
    {
        for (int j = 0; j < weaponInventory.Length; j++)
        {
            //if (weaponInventory[j].Name == "Missile Launcher")
            if (weaponInventory[j].weaponType == type)
            {
                //weaponInventory[j].weaponAmmunition += amount;
                weaponInventory[j].SupplyAmmo(amount);
                OnAmmoAmountChange?.Invoke(j, weaponInventory[j].currentAmmunition, 1.0f);
                //ui.UpdateHUDWeapons(j, weaponInventory[j].currentAmmunition);
                break;
            }
        }
    }

    public void AcceptFuel(int amount)
    {
        Debug.Log("Jump Fuel found.");
        currentJumpFuel += amount;

        OnFuelAmountChange?.Invoke(amount, maxFuelAmount, 1.0f);
        //ui.UpdateHUDFuel(currentJumpFuel, maxFuelAmount);
    }

    public float OnTickUpdate()
    {
        if (IsAttackingThisTick)
        {
            return attackWaitTime;
        }
        else
        {
            StartCoroutine(AnimateThrusterCoroutine());
            //StartCoroutine(UpdateUICoroutine());

            return moveWaitTime;
        }

        //return waitWaitTime;
    }

  
    public void NextLevel(int winFuelAmount)
    {
        currentJumpFuel = 0;
        maxFuelAmount = winFuelAmount;
        //StartCoroutine(UpdateUICoroutine());
        OnFuelAmountChange?.Invoke(0, maxFuelAmount, 1.0f);
    }

    // #Coroutines
    private IEnumerator AnimateThrusterCoroutine()
    {
        //thrusterCoroutineIsRunning = true;

        ParticleSystem ps = Thruster.GetComponent<ParticleSystem>();
        ps.Play();
        yield return new WaitForSeconds(1.0f);
        ps.Stop();

        //thrusterCoroutineIsRunning = false;
    }
    
    public IEnumerator AnimateNextLevel()
    {
        this.gameObject.SetActive(false);

        yield return new WaitForSeconds(3.0f);
    }

}
/*
    public GridObjectManager.PlayerData StoreDataForNextLevel()
    {
        GridObjectManager.PlayerData pData = new GridObjectManager.PlayerData();

        pData.amountAutoCannonAmmo = weaponInventory[0].weaponAmmunition;
        pData.amountMissileAmmo = weaponInventory[1].weaponAmmunition;
        pData.amountRailGunAmmo = weaponInventory[2].weaponAmmunition;

        return pData;
    } 
 */

/*  #DEPRECATED
    public IEnumerator UpdateUICoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        //ui.UpdateFuelHUD(currentJumpFuel, maxFuelAmount);
        ui.UpdateHUDHP(hp.CurrentHP, hp.MaxHP);
        for (int j = 0; j < weaponInventory.Length; j++)
        {
            //ui.UpdateHUDWeapons(j, weaponInventory[j].currentAmmunition);
        }

    }
    */