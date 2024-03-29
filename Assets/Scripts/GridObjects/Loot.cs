using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : GridObject
{
    [Header("Loot Item Properties")]
    private ContactSupply supply;
    private ContactFuel fuel;

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

    
    private GameObject meshObject;

    // May want to re-work this as an Init() so it can be called by a LootHandler when loot is dropped
    private void Awake()
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

    public void Init(GridBorder border)
    {
        base.Init();

        if (TryGetComponent<ContactSupply>(out ContactSupply cs))
        {
            supply = cs;
        }

        if (TryGetComponent<ContactFuel>(out ContactFuel cf))
        {
            fuel = cf;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
            {
                meshObject = renderer.gameObject;
                break;
            }
        }

        Health hp = GetComponent<Health>();
        hp.ToggleInvincibility(true);

        GridMover mp = GetComponent<GridMover>();
        switch (border)
        {
            case GridBorder.Bottom:
                mp.SetMovePatternUp();
                break;

            case GridBorder.Top:
                if (spawnRules.requiresOrientation)
                    meshObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);

                mp.SetMovePatternDown();
                break;

            case GridBorder.Right:
                if (spawnRules.requiresOrientation)
                    meshObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);

                mp.SetMovePatternLeft();
                break;

            case GridBorder.Left:
                if (spawnRules.requiresOrientation)
                    meshObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);

                mp.SetMovePatternRight();
                break;
        }

        if(TryGetComponent<Rotator>(out Rotator r))
        {
            if (!r.enabled) r.enabled = true;
            r.ApplyRotation(border);
        }
    }
}
