using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private int _currentHP;
    [SerializeField]
    public int CurrentHP
    {
        get { return _currentHP; }
    }            

    [SerializeField]
    private int maxHP;

    private void Awake()
    {
        _currentHP = maxHP;
    }


    public void SubtractHealth(int damageAmount)
    {
        if (damageAmount > 0)
        {
            _currentHP = _currentHP - damageAmount;
        }
    }

}
