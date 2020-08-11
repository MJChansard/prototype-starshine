using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private GameObject weaponGroupObject;   
    private GameObject[] weaponInventoryUI;

    
    private void Start()
    {
        if (weaponGroupObject.transform.childCount > 0)
        {
            for (int i = weaponGroupObject.transform.childCount - 1; i >= 0 ; i--)
            {
                Destroy(weaponGroupObject.transform.GetChild(i).gameObject);
            }
        }
    }

    
    public void UpdateWeaponSelection(int disableWeapon, int enableWeapon)
    {
        weaponInventoryUI[disableWeapon].transform.GetChild(0).gameObject.SetActive(false);
        weaponInventoryUI[enableWeapon].transform.GetChild(0).gameObject.SetActive(true);
    }

    public void Init(GameObject[] weaponsForUI)
    {
        weaponInventoryUI = new GameObject[weaponsForUI.Length];
        for (int i = 0; i < weaponInventoryUI.Length; i++)
        {
            weaponInventoryUI[i] = Instantiate(weaponsForUI[i], weaponGroupObject.transform);
            if (i > 0)
            {
                weaponInventoryUI[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        
    }
}
