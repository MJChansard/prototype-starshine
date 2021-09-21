using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : GridObject
{
    [Header("Loot Item Properties")]
    private ContactSupply supply;
    private ContactFuel fuel;

    //Try using an auto-property
    
    public bool DoesSupplyFuel
    {
        get
        {
            if (fuel != null) return true;
            else return false;
        }
    }
    public int FuelAmount
    {
        get
        {
            if (DoesSupplyFuel)
                return fuel.fuelAmount;
            else return 0;
        }
    }


    public bool DoesSupplyAmmo
    {
        get
        {
            if (supply != null) return true;
            else return false;
        }
    }
    public WeaponType AmmoType { get { return supply.weaponType; } }
    public int AmmoAmount { get { return supply.supplyAmount; } }

    
    private void Start()
    {
        if (TryGetComponent<ContactSupply>(out ContactSupply cs))
        {
            supply = cs;
        }

        if (TryGetComponent<ContactFuel>(out ContactFuel cf))
        {
            fuel = cf;
        }
    }
}
