using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP;    
    [SerializeField] private int currentHP;

    public int CurrentHP
    {
        get { return currentHP; }
    }
        
    public bool HasHP { get { return currentHP > 0; } }

    private bool isInvincible;

    public bool IsInvincible
    {
        get {return isInvincible; }
    }



    private void Awake()
    {
        currentHP = maxHP;
        isInvincible = false;
    }



    public void SubtractHealth(int damageAmount)
    {
        if (isInvincible == false && damageAmount > 0)
        {
            currentHP = currentHP - damageAmount;
        }
    }

    public void ToggleInvincibility(bool toggle)
    {
        isInvincible = toggle;
    }
}
