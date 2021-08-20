using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactSupply : MonoBehaviour
{
    /*
    public enum Type
    {
        AutoCannonAmmo = 1,
        MissileAmmo = 2,
        RailGunAmmo = 3
    }
    */

    //public Type supplyType;
    public WeaponType weaponType;
    public int supplyAmount;

    
    public void RequestSupply()
    {
        //supplyType = (Type)Random.Range(1, 3);
        weaponType = (WeaponType)Random.Range(1, 3);
        supplyAmount = SupplyData[weaponType];
    }

/*
    private Dictionary<Type, int> oldSupplyData = new Dictionary<Type, int>()
    {
        {Type.AutoCannonAmmo, 10},
        {Type.MissileAmmo, 6 },
        {Type.RailGunAmmo, 2 }
    };
*/

    private Dictionary<WeaponType, int> SupplyData = new Dictionary<WeaponType, int>()
    {
        { WeaponType.AutoCannon, 10 },
        { WeaponType.MissileLauncher, 6 },
        {WeaponType.RailGun, 2 }
    };

    private void Start()
    {
        RequestSupply();
    }


}
