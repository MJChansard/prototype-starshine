using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayerDisplay : MonoBehaviour
{
    private Image displayedWeapon; 

    private void Start()
    {
        displayedWeapon = GetComponent<Image>();
    }

    public void SetDisplayWeapon(Sprite currentWeapon)
    {
        displayedWeapon.sprite = currentWeapon;
    }
}
