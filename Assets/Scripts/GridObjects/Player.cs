using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Player : GridObject
{
    //  #INSPECTOR
    [BoxGroup("PLAYER PROPERTIES", centerLabel: true)]
    //[TitleGroup("PLAYER CONFIGURATION/MODULES")][SerializeField] private GameObject[] moduleInventory;
    [TitleGroup("PLAYER PROPERTIES/MODULES")] [SerializeField] private GameObject[] weaponObjects;

    [BoxGroup("PLAYER COMMANDS")]
    [Button]
    private void InflictDamage()
    {
        hp.SubtractHealth(10);
    }



    //  #PROPERTIES
    public bool IsAlive { get { return hp.HasHP; } }
    public int CountEquippedModules
    {
        get
        {
            int total = 0;
            for (int i = 0; i < equippedModules.Length; i++)
            {
                if (equippedModules[i] != null)
                    total++;
            }

            return total;
        }
    }
    public Module[] GetEquippedModules
    {
        get
        {
            Module[] array = new Module[CountEquippedModules];
            for (int i = 0; i < CountEquippedModules; i++)
            {
                if (equippedModules[i] != null)
                {
                    array[i] = equippedModules[i];
                }
            }
            return array;
        }
    }

    public GameObject currentSelectedModule;
    public Vector2Int Direction { get { return movePattern.DirectionOnGrid; } }
    public int CurrentJumpFuel { get { return currentJumpFuel; } }

    public Thruster.UsageData thrusterUsageData { get; private set; }
    public Weapon.UsageData weaponUsageData { get; private set; }
    public Shield.UsageData shieldUsageData { get; private set; }

    [HideInInspector] public bool IsAttackingThisTick = false;



    // # FIELDS
    private GameObject thruster;
    private GridMover movePattern;
    private Health hp;
    private Transform weaponSource;
    
    private Module[] equippedModules;
    private Module[] moduleInventory;
    private int moduleSelector = 0;
    
    private int currentJumpFuel = 0;
    private int maxFuelAmount = 10;

    private float attackWaitTime = 2.0f;
    private float moveWaitTime = 1.0f;
    private float waitWaitTime = 0.0f;


    public System.Action<int, int, float> OnAmmoAmountChange;
    public System.Action<int, int, float> OnFuelAmountChange;


    //  # DELEGATES
    public System.Action<int> OnActivateSelectedWeapon;



    //  # METHODS
    private void Awake()
    {
        
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject _childObject = transform.GetChild(i).gameObject;
            if(_childObject.name == "PlayerThruster")
                thruster = _childObject;

            if (_childObject.name == "WeaponSource")
                weaponSource = _childObject.transform;

            if (thruster != null && weaponSource != null)
                break;
        }
        
        movePattern = GetComponent<GridMover>();
        movePattern.SetMovePatternUp();

        hp = GetComponent<Health>();

        
        equippedModules = new Module[6];
        GameObject playerModuleTray = transform.Find("Modules").gameObject;
        moduleInventory = new Module[playerModuleTray.transform.childCount];
        for (int i = 0; i < moduleInventory.Length; i++)
        {
            moduleInventory[i] = playerModuleTray.transform.GetChild(i).GetComponent<Module>();
            EquipModule(i, moduleInventory[i]);
        }

        InputManager inputM = FindObjectOfType<InputManager>();
        inputM.ChangeDirectionButtonPressed += ChangeDirectionFacing;
        inputM.NextModuleButtonPressed += NextModule;
        inputM.PreviousModuleButtonPressed += PreviousModule;
        
        Debug.Log("Successfully subscribed to InputManager.ChangeDirectionButtonPressed");
    }
    
    //  # METHODS/Input
    void ChangeDirectionFacing(Vector2Int direction)
    {
        //Debug.Log("Player.ChangeDirectionFacing()");
        movePattern.SetMovePattern(direction);

        if(direction == Vector2Int.up)
            transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        else if(direction == Vector2Int.down)
            transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
        else if(direction == Vector2Int.left)
            transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.forward);
        else if(direction == Vector2Int.right)
            transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.forward);
    }
    void NextModule()
    {
        if (moduleSelector < moduleInventory.Length)
            moduleSelector++;
    }
    void PreviousModule()
    {
        if (moduleSelector > 0)
            moduleSelector--;
    }
    
    //  # METHODS/Modules
    public void UseCurrentModule()
    {
        if (moduleInventory[moduleSelector].UseModule())
        {
            if (moduleInventory[moduleSelector] is Thruster)
            {
                Thruster module = moduleInventory[moduleSelector] as Thruster;
                thrusterUsageData = module.LatestUsageData;
                weaponUsageData = null;
            }
            else if (moduleInventory[moduleSelector] is Weapon)
            {
                Weapon module = moduleInventory[moduleSelector] as Weapon;
                weaponUsageData = module.LatestUsageData;
                thrusterUsageData = null;
            }
        }

    }
    

    public bool EquipModule(int slot, Module module)
    {
        if (slot >= 0 && slot <= 5)
        {
            equippedModules[slot] = module;
            //module.isEquipped = true;
            return true;
        }
        else
        {
            return false;
        }
    }
       
    public void StartAttackAnimation(GridBlock gridBlock)
    {
        //weaponInventory[indexSelectedWeapon].StartAnimationCoroutine(gridBlock);
        equippedModules[moduleSelector].AnimateModule(gridBlock);
    }

    public void AcceptAmmo(WeaponType type, int amount)
    {

    }

    public void AcceptFuel(int amount)
    {
        Debug.Log("Jump Fuel found.");
        currentJumpFuel += amount;

        OnFuelAmountChange?.Invoke(amount, maxFuelAmount, 1.0f);
        //ui.UpdateHUDFuel(currentJumpFuel, maxFuelAmount);
    }

    public void OnTickUpdate()
    {
        StartCoroutine(AnimateThrusterCoroutine());
        /*
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
        */
    }

  
    public void NextLevel(int winFuelAmount)
    {
        currentJumpFuel = 0;
        maxFuelAmount = winFuelAmount;
        //StartCoroutine(UpdateUICoroutine());
        OnFuelAmountChange?.Invoke(0, maxFuelAmount, 1.0f);
    }

    // #COROUTINES
    private IEnumerator AnimateThrusterCoroutine()
    {
        //thrusterCoroutineIsRunning = true;

        ParticleSystem ps = thruster.GetComponent<ParticleSystem>();
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

/*  #DEPRECATED
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