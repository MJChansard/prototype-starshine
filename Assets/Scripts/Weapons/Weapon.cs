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
    [BoxGroup("General Weapon Properties")] public WeaponType weaponType;
    [BoxGroup("General Weapon Properties")] protected string weaponName; 
    
    virtual public int Damage { get { return weaponDamage; } }
    [BoxGroup("General Weapon Properties")] [SerializeField] protected int weaponDamage = 0;
    
    [BoxGroup("General Weapon Properties")] public int weaponAmmunition = 0;
    
    public bool DoesPenetrate { get { return doesPenetrate; } }
    [BoxGroup("General Weapon Properties")]
    [SerializeField] protected bool doesPenetrate;

    public bool RequiresGridPlacement { get { return onGridWeaponPrefab != null; } }

    public GameObject WeaponPrefab { get { return onGridWeaponPrefab; } }
    [BoxGroup("Weapon Assets")] [SerializeField] protected GameObject onGridWeaponPrefab;
    [BoxGroup("Weapon Assets")] public Sprite weaponIcon;  


    // METHODS
    public void SubtractAmmo()
    {
        weaponAmmunition--;
    }  
    
    public void SupplyAmmo(int ammoAmount)
    {
        weaponAmmunition += ammoAmount;
    }


    // Animation & Coroutines
    virtual public void StartAnimationCoroutine(GridBlock target)
    {
        StartCoroutine(AnimationCoroutine(target));
    }

    abstract protected IEnumerator AnimationCoroutine(GridBlock target);
}
