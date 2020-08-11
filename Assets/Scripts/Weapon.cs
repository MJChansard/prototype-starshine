using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected string weaponName;
    virtual public string Name
    {
        get
        {
            return weaponName;
        }
    }

    protected int weaponDamage;
    virtual public int Damage
    {
        get
        {
            return weaponDamage;
        }
    }

    protected int weaponAmmunition = 0;

    public GameObject WeaponEntryUI;

        
    abstract protected IEnumerator AnimationCoroutine(GridBlock target);

    public void StartAnimationCoroutine(GridBlock target)
    {
        StartCoroutine(AnimationCoroutine(target));
    }
}
