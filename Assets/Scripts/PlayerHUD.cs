using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private GameObject weaponGroupObject;
    //[SerializeField] private GameObject fuelGroupObject;
    [SerializeField] private Image fuelBar;
    [SerializeField] private Text fuelLabel;
    [SerializeField] private Image hpBar;
    [SerializeField] private Text hpLabel;

    private PlayerHUDEntry[] PlayerHUDEntries;
    public PlayerHUDEntry playerEntryPrefab;

    private void Start()
    {
        if (weaponGroupObject.transform.childCount > 0)
        {
            for (int i = weaponGroupObject.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(weaponGroupObject.transform.GetChild(i).gameObject);
            }
        }

        FindObjectOfType<Player>().OnPlayerFireWeapon += UpdateWeaponHUD;
    }

    private void UpdateWeaponHUD(int weaponIndex, int amount)
    {
        PlayerHUDEntries[weaponIndex].UpdateText(amount);
    }
        
    public void Init(Weapon[] weaponsForUI, int maxFuelAmount, int maxPlayerHP)
    {
        PlayerHUDEntries = new PlayerHUDEntry[weaponsForUI.Length];
        for (int i = 0; i < PlayerHUDEntries.Length; i++)
        {
            PlayerHUDEntries[i] = Instantiate(playerEntryPrefab, weaponGroupObject.transform);
            PlayerHUDEntries[i].Init(weaponsForUI[i].weaponIcon, weaponsForUI[i].currentAmmunition);

            if (i > 0)
            {
                PlayerHUDEntries[i].SetSelector(false);
            }
        }

        UpdateHUDFuel(0, maxFuelAmount);
        UpdateHUDHP(maxPlayerHP, maxPlayerHP);
    }

    public void UpdateWeaponSelection(int disableWeapon, int enableWeapon)
    {
        PlayerHUDEntries[disableWeapon].transform.GetChild(0).gameObject.SetActive(false);
        PlayerHUDEntries[enableWeapon].transform.GetChild(0).gameObject.SetActive(true);
    }

    public void UpdateHUDWeapons(int indexOfWeaponRequiringUpdate, int newAmount)
    {
        //PlayerHUDEntries[indexOfWeaponRequiringUpdate].UpdateText(newAmount);
    }

    public void UpdateHUDFuel(int currentFuelAmount, int maxFuelAmount)
    {
        fuelLabel.text = string.Format("Fuel: {0}/{1}", currentFuelAmount, maxFuelAmount);
        fuelBar.fillAmount = (float)currentFuelAmount / (float)maxFuelAmount;
    }

    public void UpdateHUDHP(int currentPlayerHP, int maxPlayerHP)
    {
        hpLabel.text = string.Format("HP: [{0}/{1}]", currentPlayerHP, maxPlayerHP);
        hpBar.fillAmount = (float)currentPlayerHP / (float)maxPlayerHP;
    }
}
