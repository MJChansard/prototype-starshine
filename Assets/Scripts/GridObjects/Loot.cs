using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : GridObject
{
    [Header("Loot Item Properties")]
    //[SerializeField] private LootType lootType;
    //[SerializeField] private int lootAmount;

    //[SerializeField]
    private ContactSupply supply;

    //[SerializeField]
    private ContactFuel fuel;
    
    public WeaponType AmmoType
    {
        get { return supply.weaponType; }
    }

    public int AmmoAmount
    {
        get { return supply.supplyAmount; }
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

    public bool DoesSupplyFuel
    {
        get
        {
            if (fuel != null) return true;
            else return false;
        }
    }

    private void Start()
    {
        if(TryGetComponent<ContactSupply>(out ContactSupply cs))
            supply = cs;

        if (TryGetComponent<ContactFuel>(out ContactFuel cf))
            fuel = cf;
    }
    /*
        public enum LootType
        {
            JumpFuel = 1,
            MissileAmmo = 2,
            RailgunAmmo = 3
        };
    */
    /*
        public LootType Type
        {
            get { return lootType; }
        }
    */
    /*
        public int LootAmount
        {
            get { return lootAmount; }
        }
    */
    /*
        public void RandomizeLoot()
        {
            int lootSelect = Random.Range(1, 11);

            if (lootSelect <= 5) lootType = LootType.MissileAmmo;
            else lootType = LootType.RailgunAmmo;
        }
    */
}
