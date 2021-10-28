using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum WeaponType
{
    AutoCannon = 1,
    MissileLauncher = 2,
    RailGun = 3
};


public abstract class Weapon : MonoBehaviour
{
    virtual public string Name { get { return weaponName; } }
    [BoxGroup("WEAPON CONFIGURATION", centerLabel:true)] public WeaponType weaponType;
    [BoxGroup("WEAPON CONFIGURATION")] protected string weaponName; 
        
    virtual public int Damage { get { return baseDamage; } }
    [BoxGroup("WEAPON CONFIGURATION")] [SerializeField] protected int baseDamage;
    [BoxGroup("WEAPON CONFIGURATION")] [SerializeField] protected float gridDistanceDamageModifier;
        
    public bool DoesPenetrate { get { return doesPenetrate; } }
    [BoxGroup("WEAPON CONFIGURATION")] [SerializeField] protected bool doesPenetrate;

    public bool RequiresGridPlacement { get { return requiresGridPlacement; } }
    [BoxGroup("WEAPON CONFIGURATION")] [SerializeField] protected bool requiresGridPlacement;
    
    public GameObject WeaponPrefab { get { return onGridWeaponPrefab; } }
    [BoxGroup("WEAPON ASSETS", centerLabel:true)] public Sprite weaponIcon;
    [ShowIf("requiresGridPlacement")][BoxGroup("WEAPON ASSETS")] [SerializeField] protected GameObject onGridWeaponPrefab;

    [BoxGroup("WEAPON PROPERTIES")] public int startingAmmunition;
    [BoxGroup("WEAPON PROPERTIES")] public int maxAmmunition;
    [BoxGroup("WEAPON PROPERTIES", centerLabel:true)][DisplayAsString] public int currentAmmunition;
    

    

    // METHODS
    public int CalculateDamage(int gridBlockDistance, bool verboseConsole = false)
    {
        /*  NOTE ON gridBlockDistance
         *    If the Player occupies a GridBlock that is immediately adjacent to the target, the distance = 0
         *    If there is one GridBlock in between the Player and the target, the distance = 1
         *    If there are two GridBlocks in between the Player and the target, the distance = 2
         *    If there are three GridBlocks in between the Player and the target, the distance = 3
         */

        if (verboseConsole)
        {
            Debug.LogFormat("Grid Block Distance received: {0}", gridBlockDistance);
            Debug.LogFormat("Grid Modifier: {0}", gridDistanceDamageModifier);
            Debug.LogFormat("Base Damage: {0}", baseDamage);
        }
    
        float modDamageValue = Mathf.Pow((float)gridDistanceDamageModifier, (float)gridBlockDistance);
        if (verboseConsole)
            Debug.LogFormat("Damage modifier value: {0}", modDamageValue.ToString());
        
        float newDamageValue = baseDamage * modDamageValue;
        if (verboseConsole)
            Debug.LogFormat("Damage modifier value: {0}", newDamageValue.ToString());
        
        return (int)newDamageValue;
    }
    public void SubtractAmmo()
    {
        currentAmmunition--;
    }  
    public void SupplyAmmo(int ammoAmount)
    {
        currentAmmunition += ammoAmount;
    }


    // ANIMATION COROUTINE
    virtual public void StartAnimationCoroutine(GridBlock target)
    {
        StartCoroutine(AnimationCoroutine(target));
    }

    abstract protected IEnumerator AnimationCoroutine(GridBlock target);
}
