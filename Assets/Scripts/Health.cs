using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public System.Action<int, int, float> OnHpAmountChange;

    [SerializeField] private int maxHP;    
    [SerializeField] private int currentHP;

    public int MaxHP { get { return maxHP; } }
    public int CurrentHP { get { return currentHP; } }
    public bool HasHP { get { return currentHP > 0; } }
    
        

    public bool IsInvincible
    {
        get {return isInvincible; }
    }
    private bool isInvincible;


    private void Awake()
    {
        currentHP = maxHP;
        isInvincible = false;
    }


    public void SubtractHealth(int damageAmount)
    {
        if (isInvincible == false && damageAmount > 0)
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
        isInvincible = toggle;
    }
}
