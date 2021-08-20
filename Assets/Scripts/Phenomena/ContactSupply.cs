using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ContactSupply : MonoBehaviour
{
    public WeaponType weaponType;
    public int supplyAmount;
    public bool randomizeSupply;
    public bool randomizeNumberResupplyRemaining;
    [ShowIf("@!randomizeNumberResupplyRemaining")]
    public int numberResupplyRemaining;
      
   
    private Dictionary<WeaponType, int> SupplyData = new Dictionary<WeaponType, int>()
    {
        { WeaponType.AutoCannon, 10 },
        { WeaponType.MissileLauncher, 6 },
        {WeaponType.RailGun, 2 }
    };


    private void Start()
    {
        if (randomizeSupply)
            RequestSupply();
    }

    public void RequestSupply()
    {
        weaponType = (WeaponType)Random.Range(1, 3);
        supplyAmount = SupplyData[weaponType];
    }
}
