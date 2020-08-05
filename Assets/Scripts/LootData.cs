using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootData : MonoBehaviour
{
    [Header("Loot Item Properties")]
    [SerializeField] private string lootItemName;
    [SerializeField] private int jumpFuelValue;
    [SerializeField] private int missileAmmoValue;
    [SerializeField] private int railgunAmmoValue;

    public string LootItemName
    {
        get
        {
            return lootItemName;
        }
    }

    public int JumpFuelIncrement
    {
        get { return jumpFuelValue; }
    }
}
