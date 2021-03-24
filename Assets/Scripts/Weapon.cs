using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected string weaponName;
    virtual public string Name { get { return weaponName; } }
        
    virtual public int Damage { get { return weaponDamage; } }
    [SerializeField] protected int weaponDamage = 0;

    public bool DoesPenetrate { get { return doesPenetrate; } }
    [SerializeField] protected bool doesPenetrate;

    public bool RequiresInstance { get { return projectilePrefab != null; } }
    [SerializeField] protected GameObject projectilePrefab;
    public GameObject WeaponPrefab { get { return projectilePrefab; } }

    public int weaponAmmunition = 0;

    public Sprite weaponIcon;


    virtual public void StartAnimationCoroutine(GridBlock target)
    {
        StartCoroutine(AnimationCoroutine(target));
    }

    abstract protected IEnumerator AnimationCoroutine(GridBlock target);
}
