using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootData : MonoBehaviour
{
    [Header("Loot Item Properties")]
    [SerializeField] private LootType lootType;
    [SerializeField] private int jumpFuelValue;
    [SerializeField] private int missileAmmoValue;
    [SerializeField] private int railgunAmmoValue;

    public enum LootType
    {
        JumpFuel = 1,
        MissileAmmo = 2,
        RailgunAmmo = 3
    };

    public LootType Type
    {
        get { return lootType; }
    }

    public int JumpFuelIncrement
    {
        get { return jumpFuelValue; }
    }

    public int MissileAmmoIncrement
    {
        get { return missileAmmoValue; }
    }

    public int RailgunAmmoIncrement
    {
        get { return railgunAmmoValue; }
    }

}
