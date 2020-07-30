using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayerDisplay : MonoBehaviour
{   
    public void SetDisplayWeapon(Sprite currentWeapon)
    {
        displayedWeapon.sprite = currentWeapon;
    }
}
