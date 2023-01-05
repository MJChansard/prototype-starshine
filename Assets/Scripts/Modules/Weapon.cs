using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Weapon : Module
{
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

    public new class InitializationData : Module.InitializationData
    {
        public AmmunitionType ammoType { get; set; }
        public int ammoCapacity { get; set; }
        public int ammoAvailable { get; set; }
    }

    public new class UsageData : Module.UsageData
    {
        public int BaseDamage { get; set; }
        public bool DoesPenetrate { get; set; }
        public bool DynamicDamage { get; set; }
        public float DamageMultiplier { get; set; }

        public bool DoesPlaceObjectInWorld { get; set; }
        public GameObject ObjectToPlace { get; set; }
                
        public UsageData(int baseDamage, bool doesPenetrate, float damageMultipler,  bool placesObject, GameObject objectToPlace = null)
        {
            BaseDamage = baseDamage;
            DoesPenetrate = doesPenetrate;
            DynamicDamage = DamageMultiplier > 0.0;
            DamageMultiplier = damageMultipler;
            DoesPlaceObjectInWorld = placesObject;
            ObjectToPlace = objectToPlace;
        }
    }

    // #INSPECTOR
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool doesDamage;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool doesPlaceObjectInWorld;
    
    [ShowIf("doesPlaceObjectInWorld")][TitleGroup("ASSETS")][SerializeField] private GameObject objectPlacedInWorld;

    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private WeaponType weaponType;
    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private int baseDamage;
    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private float damageMultiplier;
    [ShowIf("doesDamage")][TitleGroup("WEAPON SETTINGS")][SerializeField] private bool doesPenetrate;

    [TitleGroup("WEAPON DEBUGGING")][SerializeField] private bool verboseLogging;
    [TitleGroup("WEAPON DEBUGGING")][SerializeField] private bool verboseInitializationLogging;


    // Might need a new field indicating whether grid distance impacts damage?


    // #PROPERTIES
    public bool isEquipped { get; set; }
    public Weapon.InitializationData HUDInitializationData
    {
        get
        {
            var data = new Weapon.InitializationData
            {
                availableIcon = availableIcon,
                useIcon = useIcon,
                cooldownIcon = cooldownIcon,

                ammoType = ammoType,
                ammoCapacity = capacityAmmo,
                ammoAvailable = currentAmmo
            };

            return data;
        }
    }
    public UsageData LatestUsageData { get; private set; }


    // #FIELDS
    private bool isActive;
    private ParticleSystem ps;
    private int currentAmmo;


    private void Awake()
    {
        if (hasAnimation)
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Stop();
            else
                Debug.Log("Prefab configuration error!  No ParticleSystem found although an animation is expected.");
        }
        currentAmmo = startAmmo;
        displayCurrentAmmoInspector = currentAmmo;
    }


    //  #METHODS
    public override bool UseModule()
    {
        if (ammoType != AmmunitionType.None)
        {
            bool sufficientResource = ConsumeResource(ammoType);

            LatestUsageData = new Weapon.UsageData
            (
                baseDamage: this.baseDamage,
                doesPenetrate: this.doesPenetrate,
                damageMultipler: this.damageMultiplier,
                placesObject: this.doesPlaceObjectInWorld,
                objectToPlace: this.objectPlacedInWorld
            );
            
            if (VerboseConsole)
                Debug.LogFormat("Module: {0} usage successful.", moduleName);

            // #TODO UI needs to be updated

            return sufficientResource;
        }
        else
        {
            if (VerboseConsole)
                Debug.LogFormat("Module {0} usage unsuccessful.  Insufficient resource.", moduleName);
            return false;
        }
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

    public IEnumerator AnimateCoroutine(GridBlock gb)
    {
        if (gb.objectsOnBlock.Count > 0)
        {
            for (int i = 0; i < gb.objectsOnBlock.Count; i++)
            {
                if (gb.objectsOnBlock[i].TryGetComponent<GridObject>(out GridObject gridObject))
                {
                    var trigger = ps.trigger;
                    trigger.enabled = true;

                    trigger.SetCollider(0, gridObject.GetComponent<Collider>());
                    trigger.radiusScale = 0.5f;

                    ps.Play();
                    yield return new WaitForSeconds(2.0f);
                    ps.Stop();

                    break;
                }
            }
        }
        else
        {
            ps.Play();
            yield return new WaitForSeconds(2.0f);
            ps.Stop();
        }
    }
}
