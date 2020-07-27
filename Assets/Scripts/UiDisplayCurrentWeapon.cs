using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiDisplayCurrentWeapon : MonoBehaviour
{
    
    [SerializeField] Sprite[] weaponInventory;

    private Image displayedWeapon;
    private int weaponInventoryIndex;
    

    private void Start()
    {
        weaponInventoryIndex = 0;
        displayedWeapon = GetComponent<Image>();
        displayedWeapon.sprite = weaponInventory[weaponInventoryIndex];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && weaponInventoryIndex > 0) weaponInventoryIndex -= 1;

        if (Input.GetKeyDown(KeyCode.E) && weaponInventoryIndex < 2) weaponInventoryIndex += 1;

        displayedWeapon.sprite = weaponInventory[weaponInventoryIndex];
    }
}
