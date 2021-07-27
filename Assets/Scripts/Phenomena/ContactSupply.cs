using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactSupply : MonoBehaviour
{
    public enum Type
    {
        AutoCannonAmmo = 1,
        MissileAmmo = 2,
        RailGunAmmo = 3
    }

    public int supplyAmount;

    public Type RandomSelect()
    {
        return (Type)Random.Range(1, 3);
    }
}
