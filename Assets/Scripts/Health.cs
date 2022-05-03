using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Health : MonoBehaviour
{
    public System.Action<int, int, float> OnHpAmountChange;

    [SerializeField] int maxHP;    
    [ShowInInspector][DisplayAsString] int currentHP;

    public int MaxHP { get { return maxHP; } }
    public int CurrentHP { get { return currentHP; } }
    public bool HasHP
    {
        get
        {
            if (currentHP > 0)
                return true;
            else if (IsInvincible)
                return true;
            else
                return false;
        } 
    }
      

    [ShowInInspector] public bool IsInvincible { get; private set; }
    
    private void Awake()
    {
        currentHP = maxHP;
    }


    public void SubtractHealth(int damageAmount)
    {
        if (IsInvincible == false && damageAmount > 0)
        {
            currentHP -= damageAmount;
            OnHpAmountChange?.Invoke(currentHP, maxHP, 1.0f);
        }
    }
    public void AddHealth(int healthAmount)
    {
        while (currentHP < maxHP && healthAmount > 0)
        {
            currentHP++;
            healthAmount--;
        }

        OnHpAmountChange(currentHP, maxHP, 1.0f);
    }

    public void ToggleInvincibility(bool toggle)
    {
        IsInvincible = toggle;
    }
}
