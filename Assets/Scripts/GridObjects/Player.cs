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
            return equippedModules.Length;
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
    public Vector2Int Direction { get { return thruster.CurrentDirectionFacing; } }
    public int CurrentJumpFuel { get { return currentJumpFuel; } }

    public Thruster.UsageData thrusterUsageData { get; private set; }
    public Weapon.UsageData weaponUsageData { get; private set; }
    public Shield.UsageData shieldUsageData { get; private set; }
    
    [HideInInspector] public bool IsAttackingThisTick = false;



    // # FIELDS
    private Health hp;
    private Transform weaponSource;
    
    private Module[] equippedModules;
    private List<Module> moduleInventory;
    private Thruster thruster;
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
    

    
    private void Awake()
    {
        moduleInventory = new List<Module>();
        /*
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject _childObject = transform.GetChild(i).gameObject;

            if (_childObject.name == "WeaponSource")
            {
                weaponSource = _childObject.transform;
            }
                       else if (_childObject.name == "Modules")
                       {
                           moduleInventory = new Module[_childObject.transform.childCount];
                           for (int j = 0; j < moduleInventory.Length; j++)
                           {
                               Module m = _childObject.transform.GetChild(j).GetComponent<Module>();
                               moduleInventory[j] = m;
                               EquipModule(j, m);
                               if (m is Thruster)
                                   thruster = m as Thruster;
                           }
                       }
            */

        hp = GetComponent<Health>();  

        InputManager inputM = FindObjectOfType<InputManager>();
        inputM.ChangeDirectionButtonPressed += ChangeDirectionFacing;
        inputM.NextModuleButtonPressed += NextModule;
        inputM.PreviousModuleButtonPressed += PreviousModule;
        
        Debug.Log("Successfully subscribed to InputManager.ChangeDirectionButtonPressed");
    }

    public override void Init()
    {
        base.Init();

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject _childObject = transform.GetChild(i).gameObject;

            if (_childObject.name == "WeaponSource")
            {
                weaponSource = _childObject.transform;
            }
            else if (_childObject.CompareTag("Module"))
            {
                Debug.LogFormat("Found module: {0}", _childObject.name);

                Module m = _childObject.GetComponent<Module>();
                if (m != null)
                    moduleInventory.Add(m);
                else
                    Debug.Log("Prefab configuration error.  Prefab is tagged with 'Module' but lacks a 'Module' component.");
            }
        }
        
        equippedModules = new Module[moduleInventory.Count];
        for (int i = 0; i < moduleInventory.Count; i++)
        {
            Module m = moduleInventory[i];
            EquipModule(i, m);
            if (m is Thruster)
                thruster = m as Thruster;
        }
    }
    //  # METHODS/Input
    void ChangeDirectionFacing(Vector2Int direction)
    {
        thruster.CurrentDirectionFacing = direction;
        Debug.LogFormat("Player direction Vector2Int: {0}", thruster.CurrentDirectionFacing.ToString());

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
        if (moduleSelector < equippedModules.Length)
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
        bool moduleIsAvailable = equippedModules[moduleSelector].UseModule();
        if (moduleIsAvailable)
        {
            Debug.LogFormat("Using current module: {0}", equippedModules[moduleSelector].name);
            if (equippedModules[moduleSelector] is Thruster)
            {
                //Module m = moduleInventory[moduleSelector];
                //Thruster t = m as Thruster;
                Thruster t = equippedModules[moduleSelector] as Thruster;
                thrusterUsageData = t.LatestUsageData;
                weaponUsageData = null;
            }
            else if (equippedModules[moduleSelector] is Weapon)
            {
                Weapon module = equippedModules[moduleSelector] as Weapon;
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
        //equippedModules[moduleSelector].AnimateModule(gridBlock);
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