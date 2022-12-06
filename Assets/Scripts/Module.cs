using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Module : MonoBehaviour
{
    public class InitializationData
    {
        public Sprite availableIcon;
        public Sprite useIcon;
        public Sprite cooldownIcon;
        public AmmunitionType ammoType;
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

    public class MoveData
    {

    }


    public enum ResourceType
    {
        None = 0,
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
  
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private string moduleName;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private AmmunitionType ammoType;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool hasCoolDown;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool doesDamage;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool doesPlaceObjectInWorld;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool hasAnimation;


    [TitleGroup("ASSETS")][SerializeField] private Sprite availableIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite useIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite cooldownIcon;
    [ShowIf("doesPlaceObjectInWorld")][TitleGroup("ASSETS")][SerializeField] private GameObject objectPlacedInWorld;


    [ShowIf("hasCoolDown")][TitleGroup("COOLDOWN SETTINGS")][SerializeField] private int cooldownLength;
    [ShowIf("hasCoolDown")][TitleGroup("COOLDOWN SETTINGS")][DisplayAsString] private int cooldownCounter;


    [ShowIf("@ammoType != AmmunitionType.None")][TitleGroup("AMMUNITION SETTINGS")][SerializeField] private int capacityAmmo;
    [ShowIf("@ammoType != AmmunitionType.None")][TitleGroup("AMMUNITION SETTINGS")][SerializeField] private int startAmmo;
    [ShowIf("@ammoType != AmmunitionType.None")][TitleGroup("AMMUNITION SETTINGS")][SerializeField] private int usageAmmoCost;
    [ShowIf("@ammoType != AmmunitionType.None")][TitleGroup("AMMUNITION SETTINGS")][DisplayAsString] private int displayCurrentAmmoInspector;

    
    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private WeaponType weaponType;
    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private int baseDamage;
    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private float damageMultiplier;
    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private bool doesPenetrate;

    // Might need a new field indicating whether grid distance impacts damage?


    // #PROPERTIES
    
    public bool isEquipped { get; set; }
    public InitializationData Data
    {
        get
        {
            InitializationData data = new InitializationData();

            data.availableIcon = availableIcon;
            data.useIcon = useIcon;
            data.cooldownIcon = cooldownIcon;
            data.ammoType = ammoType;
            data.capacityAmmo = capacityAmmo;

            return data;
        }
    }

    // #FIELDS
    private bool isActive;
    private IModuleAnimator animator;
    private int currentAmmo;


    private void Awake()
    {
        if(hasAnimation)
        {
            animator = GetComponent<IModuleAnimator>();
            if (TryGetComponent<ParticleSystem>(out ParticleSystem ps))
                ps.Stop();
        }
        currentAmmo = startAmmo;
        displayCurrentAmmoInspector = currentAmmo;
    }


    //  #METHODS
    public bool UseModule(out UsageData data)
    {
        /*  NOTES
         * 
         *  - If this method needs to be able to handle a Thruster & a Shield module then
         *      there is a lot of work that needs to be done here
         *      
         *  - Also the UsageData class becomes really useless in its current form since it is
         *      so tailored to weapons
         */
        if (ammoType != AmmunitionType.None)
        {
            bool sufficientResource = ConsumeResource(ammoType);

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
                Debug.LogFormat("Module: {0} usage successful.", moduleName);

            // #TODO UI needs to be updated

            return sufficientResource;
        } 
        else
        {
            data = new UsageData();
            if (VerboseConsole)
                Debug.LogFormat("Module {0} usage unsuccessful.  Insufficient resource.", moduleName);
            return false; 
        }
    }

    public void AnimateModule(GridBlock gb)
    {
        //StartCoroutine(animator.TriggerModuleAnimationCoroutine(gb));
        animator.StartModuleAnimationCoroutine(gb);
    }
    
    public void UpdateCounter()
    {
         
    }

    private bool ConsumeResource(AmmunitionType r)
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

    public void SupplyResource(AmmunitionType r)
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