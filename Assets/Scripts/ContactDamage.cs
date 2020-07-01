using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    [SerializeField] private int damageAmount;

    public int DamageAmount
    {
        get { return damageAmount; }
    }
}
