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

        Player p = FindObjectOfType<Player>();
        p.OnAmmoAmountChange += UpdateWeaponHUD;
        p.OnFuelAmountChange += UpdateFuelHUD;
        p.GetComponent<Health>().OnHpAmountChange += UpdateHpHUD;
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

        UpdateFuelHUD(0, maxFuelAmount, 0.0f);
        UpdateHpHUD(maxPlayerHP, maxPlayerHP, 0.0f);
    }

    public void UpdateWeaponSelection(int disableWeapon, int enableWeapon)
    {
        PlayerHUDEntries[disableWeapon].transform.GetChild(0).gameObject.SetActive(false);
        PlayerHUDEntries[enableWeapon].transform.GetChild(0).gameObject.SetActive(true);
    }

    private void UpdateWeaponHUD(int weaponIndex, int amount, float animateDelay)
    {
        StartCoroutine(UpdateWeaponHUDCoroutine(weaponIndex, amount, animateDelay));
    }

    /*  #Deprecated
    public void UpdateHUDWeapons(int indexOfWeaponRequiringUpdate, int newAmount)
    {
        //PlayerHUDEntries[indexOfWeaponRequiringUpdate].UpdateText(newAmount);
    }
    */

    public void UpdateFuelHUD(int currentFuelAmount, int maxFuelAmount, float animateDelay)
    {
        StartCoroutine(UpdateFuelHUDCoroutine(currentFuelAmount, maxFuelAmount, animateDelay));
    }

    public void UpdateHpHUD(int currentPlayerHP, int maxPlayerHP, float animateDelay)
    {
        StartCoroutine(UpdateHpHUDCoroutine(currentPlayerHP, maxPlayerHP, animateDelay));
    }

    private IEnumerator UpdateWeaponHUDCoroutine(int weaponIndex, int newAmount, float numberSeconds)
    {
        yield return new WaitForSeconds(numberSeconds);
        PlayerHUDEntries[weaponIndex].UpdateText(newAmount);
    }

    private IEnumerator UpdateFuelHUDCoroutine(int currentFuel, int maxFuel, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        fuelLabel.text = string.Format("Fuel: {0}/{1}", currentFuel, maxFuel);
        fuelBar.fillAmount = (float)currentFuel / (float)maxFuel;
    }

    private IEnumerator UpdateHpHUDCoroutine(int currentHP, int maxHP, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        hpLabel.text = string.Format("HP: [{0}/{1}]", currentHP, maxHP);
        hpBar.fillAmount = (float)currentHP / (float)maxHP;
    }    
}
