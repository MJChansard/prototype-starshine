using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactSupply : MonoBehaviour
{
    public WeaponType weaponType;
    public int supplyAmount;

    
    public void RequestSupply()
    {
        weaponType = (WeaponType)Random.Range(1, 3);
        supplyAmount = SupplyData[weaponType];
    }

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
