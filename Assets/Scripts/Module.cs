using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Module : MonoBehaviour
{
    public class InitializationData
    {
        public Sprite activeIcon;
        public Sprite inactiveIcon;
        public ResourceType resourceType;
        public int capacityAmmo;
    }

    public class UsageData
    {
        public bool doesDamage;
        public bool doesPlaceObjectInWorld;
        //public bool hasAnimation;
        public bool doesPenetrate;

        public int baseDamage;
        public bool dynamicDamage;
        public float damageMultiplier;

        public GameObject objectToPlaceInWorld;

        public int newAmmoAmount;
    }
    // Don't keep two different things
    // Look into public properties approach
    // Or could have module-lite serialized class that doesn't Inherit from MonoBehavior

    public enum Type
    {
        Functional = 1,
        Augment = 2
    }

    public enum ResourceType
    {
        Ammunition = 1,
        Battery = 2
    }

    public enum WeaponType
    {
        AutoCannon = 1,
        MissileLauncher = 2,
        RailGun = 3
    }


    // #INSPECTOR
    [SerializeField] private bool VerboseConsole;

    [BoxGroup("GENERAL MODULE SETTINGS", centerLabel: true)]
    [TitleGroup("GENERAL MODULE SETTINGS/CONFIGURATION")] [SerializeField] private string moduleName;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private Type mode;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private ResourceType resourceType;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private int resourceCost;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private bool hasCoolDown;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private bool doesDamage;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private bool doesPlaceObjectInWorld;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private bool hasAnimation;

    [TitleGroup("GENERAL MODULE SETTINGS/ASSETS")]
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private Sprite activeIcon;
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private Sprite inactiveIcon;

    [ShowIf("doesPlaceObjectInWorld")]
    [BoxGroup("GENERAL MODULE SETTINGS")] [SerializeField] private GameObject objectPlacedInWorld;

    [ShowIf("doesUseAmmo")] [BoxGroup("AMMO RESOURCE CONFIGURATION", centerLabel: true)] [SerializeField]   private int capacityAmmo;
    [ShowIf("doesUseAmmo")] [BoxGroup("AMMO RESOURCE CONFIGURATION")] [SerializeField]                      private int startAmmo;
    [ShowIf("doesUseAmmo")] [BoxGroup("AMMO RESOURCE CONFIGURATION")] [DisplayAsString]                     private int displayAmmoInspector;

    [ShowIf("hasCoolDown")] [BoxGroup("COOLDOWN CONFIGURATION", centerLabel: true)] [SerializeField]    private int cooldownLength;
    [ShowIf("hasCoolDown")] [BoxGroup("COOLDOWN CONFIGURATION")] [DisplayAsString]                      private int cooldownCounter;

    [ShowIf("doesDamage")] [BoxGroup("WEAPON CONFIGURATION", centerLabel: true)] [SerializeField]   private WeaponType weaponType;
    [ShowIf("doesDamage")] [BoxGroup("WEAPON CONFIGURATION")] [SerializeField]                      private int baseDamage;
    [ShowIf("doesDamage")] [BoxGroup("WEAPON CONFIGURATION")] [SerializeField]                      private float damageMultiplier;
    [ShowIf("doesDamage")] [BoxGroup("WEAPON CONFIGURATION")] [SerializeField]                      private bool doesPenetrate;

    // Might need a new field indicating whether grid distance impacts damage?


    // #PROPERTIES
    public bool doesUseAmmo { get { return resourceType == ResourceType.Ammunition; } }
    public bool doesUseBattery { get { return resourceType == ResourceType.Battery; } }
    public bool isEquipped { get; set; }
    public InitializationData Data
    {
        get
        {
            InitializationData data = new InitializationData();

            data.activeIcon = activeIcon;
            data.inactiveIcon = inactiveIcon;
            data.resourceType = resourceType;
            data.capacityAmmo = capacityAmmo;

            return data;
        }
    }

    // #FIELDS
    private bool isActive;
    private ModuleAnimator animator;
    private int currentAmmo;


    private void Awake()
    {
        if(hasAnimation)
        {
            animator = GetComponent<ModuleAnimator>();
        }
        currentAmmo = startAmmo;
        displayAmmoInspector = currentAmmo;
    }


    //  #METHODS
    public bool UseModule(out UsageData data)
    {
        bool sufficientResource = ConsumeResource(resourceType);
        data = new UsageData
        {
            doesDamage = this.doesDamage,
            baseDamage = this.baseDamage,
            dynamicDamage = damageMultiplier > 0.0,
            damageMultiplier = this.damageMultiplier,

            doesPenetrate = this.doesPenetrate,

            doesPlaceObjectInWorld = this.doesPlaceObjectInWorld,
            objectToPlaceInWorld = this.objectPlacedInWorld,

            //hasAnimation = this.hasAnimation,

            newAmmoAmount = currentAmmo
        };

        if (VerboseConsole)
        {
            if (sufficientResource)
                Debug.LogFormat("Module: {0} usage successful.", moduleName);
            else
                Debug.LogFormat("Module {0} usage unsuccessful.  Insufficient resource.", moduleName);
        }
        
        // UI needs to be updated

        return sufficientResource;
    }

    public void AnimateModule(GridBlock gb)
    {
        StartCoroutine(animator.TriggerModuleAnimationCoroutine(gb));
    }
    
    public void UpdateCounter()
    {
         
    }

    private bool ConsumeResource(ResourceType r)
    {
        if (r == ResourceType.Ammunition)
        {
            if (currentAmmo > 0)
            {
                currentAmmo--;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }       
    }

    public void SupplyResource(ResourceType r)
    {

    }

    public void StartCooldown()
    {

    }

    public void ToggleModuleOnOff()
    {

    }
}

// END OF FILE