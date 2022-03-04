using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ContactSupply : MonoBehaviour
{
    public bool randomizeSupply;
    [ShowIf("@!randomizeSupply")]
    public WeaponType weaponType;

    [SerializeField] [DisplayAsString] private WeaponType SupplyContents;
   
    [ShowIf("@!randomizeSupply")]
    public int supplyAmount;
    
    public bool randomizeNumberResupplyRemaining;
    [ShowIf("@!randomizeNumberResupplyRemaining")]
    public int numberResupplyRemaining;
      
   
    private Dictionary<WeaponType, int> SupplyData = new Dictionary<WeaponType, int>()
    {
        { WeaponType.AutoCannon, 10 },
        { WeaponType.MissileLauncher, 6 },
        { WeaponType.RailGun, 2 }
    };


    private void Start()
    {
        if (randomizeSupply)
            RequestSupply();
        SupplyContents = weaponType;
    }

    public void RequestSupply()
    {
        weaponType = (WeaponType)Random.Range(1, 4);
        supplyAmount = SupplyData[weaponType];
    }

    public void ConsumeSupply()
    {
        numberResupplyRemaining--;
    }
}
