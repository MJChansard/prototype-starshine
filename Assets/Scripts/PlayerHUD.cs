using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private GameObject weaponGroupObject;   
    private PlayerHUDEntry[] PlayerHUDEntries;
    public PlayerHUDEntry playerEntryPrefab;

    
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
        PlayerHUDEntries[disableWeapon].transform.GetChild(0).gameObject.SetActive(false);
        PlayerHUDEntries[enableWeapon].transform.GetChild(0).gameObject.SetActive(true);
    }

    
    public void Init(Weapon[] weaponsForUI)
    {
        PlayerHUDEntries = new PlayerHUDEntry[weaponsForUI.Length];
        for (int i = 0; i < PlayerHUDEntries.Length; i++)
        {
            PlayerHUDEntries[i] = Instantiate(playerEntryPrefab, weaponGroupObject.transform);
            PlayerHUDEntries[i].Init(weaponsForUI[i].weaponIcon, weaponsForUI[i].weaponAmmunition);

            if (i > 0)
            {
                PlayerHUDEntries[i].SetSelector(false);
            }
        }

        
    }
}
