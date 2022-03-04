using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    //  #INSPECTOR
    [SerializeField] private bool VerboseLogging;
    [SerializeField] private Image hpBar;
    [SerializeField] private Text hpLabel;
    [SerializeField] private Image batteryBar;
    [SerializeField] private Text batteryLabel;
    [SerializeField] private Image fuelBar;
    [SerializeField] private Text fuelLabel;
    [SerializeField] private PlayerHUDEntry playerHUDEntryPrefab;

    

    //  #FIELDS
    private GameObject modulesGroupHUD;
    private PlayerHUDEntry[] entryArrayHUD;// = new PlayerHUDEntry[6];
    private int entryArrayHUDIndex;
    //  References
    private InputManager inputM;

    // #DELEGATES
    public System.Action OnPlayerAdvance;
    public System.Action<Module.UsageData> OnPlayerHUDModuleActivate;

    private void Awake()
    {
        entryArrayHUDIndex = 0;

        if (VerboseLogging)
            Debug.Log("PlayerHUD.Awake() commencing.");

        modulesGroupHUD = GameObject.FindWithTag("Modules HUD");

        if (VerboseLogging && modulesGroupHUD == null)
            Debug.Log("PlayerHUD failed to locate Modules HUD GameObject.");
        else
            Debug.Log("PlayerHUD successfully located Modules HUD GameObject.");

        if (modulesGroupHUD.transform.childCount > 0)
        {
            for (int i = modulesGroupHUD.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(modulesGroupHUD.transform.GetChild(i).gameObject);
            }
        }

       // entry = modulesGroupHUD?.GetComponentInChildren<PlayerHUDEntry>();      // Try clearing the reference in Inspector
        //if (entry == null)
        //    Debug.Log("Did you forget to assign the Weapon HUD Entry Object to PlayerHUD?");


        inputM = FindObjectOfType<InputManager>();
        if (inputM != null)
        {
            inputM.NextModuleButtonPressed += SelectNextModule;
            inputM.PreviousModuleButtonPressed += SelectPreviousModule;
        }
        else
        {
            //Debug.Log("Did you forget to place the Input Manager Component?");
        }
    }

    
    public void Init(Module[] modulesForUI, int maxFuelAmount, int maxPlayerHP)   
    {
        Player p = FindObjectOfType<Player>();
        if (p != null) Debug.Log("PlayerHUD found Player!.");


        entryArrayHUD = new PlayerHUDEntry[modulesForUI.Length];
        for (int i = 0; i < entryArrayHUD.Length; i++)
        {
            PlayerHUDEntry entry = Instantiate(playerHUDEntryPrefab, modulesGroupHUD.transform);
            if (entry != null)
            {
                entryArrayHUD[i] = entry;
                entry.Init(modulesForUI[i].Data);
                entry.ReferenceToModuleOnPlayer = modulesForUI[i];
            }                    
            else
            {
                Debug.Log("entry is null.");
            }      
            
            if (i > 0)
            {
                entryArrayHUD[i].SetSelector(false);
            }
        }

        UpdateFuelHUD(0, maxFuelAmount, 0.0f);
        UpdateHpHUD(maxPlayerHP, maxPlayerHP, 0.0f);
    }
    

    /*  ALTERNATE METHOD
    public void Init(Weapon weaponForUI, int maxFuelAmount, int maxPlayerHP)
    {
        weaponEntry.GetComponentInChildren<Image>().sprite = weaponForUI.weaponIcon;
        weaponEntry.GetComponentInChildren<Text>().text = weaponForUI.currentAmmunition.ToString();

        UpdateFuelHUD(0, maxFuelAmount, 0.0f);
        UpdateHpHUD(maxPlayerHP, maxPlayerHP, 0.0f);
    }
    */

    public void SelectNextModule()
    {
        entryArrayHUD[entryArrayHUDIndex].SetSelector(false);  
        if(entryArrayHUDIndex < entryArrayHUD.Length)
            entryArrayHUDIndex++;
        entryArrayHUD[entryArrayHUDIndex].SetSelector(true);
    }
    public void SelectPreviousModule()
    {
        entryArrayHUD[entryArrayHUDIndex].SetSelector(false);
        if (entryArrayHUDIndex > 0)
            entryArrayHUDIndex--;
        entryArrayHUD[entryArrayHUDIndex].SetSelector(true);
    }

    public void RefreshHUDEntry(int newAmmoAmount, bool currentlySelected)
    {
        entryArrayHUD[entryArrayHUDIndex].Refresh(newAmmoAmount, currentlySelected);
    }


    public void UpdateFuelHUD(int currentFuelAmount, int maxFuelAmount, float animateDelay)
    {
        StartCoroutine(UpdateFuelHUDCoroutine(currentFuelAmount, maxFuelAmount, animateDelay));
    }
    public void UpdateHpHUD(int currentPlayerHP, int maxPlayerHP, float animateDelay)
    {
        StartCoroutine(UpdateHpHUDCoroutine(currentPlayerHP, maxPlayerHP, animateDelay));
    }


    private IEnumerator UpdateFuelHUDCoroutine(int currentFuel, int maxFuel, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        fuelLabel.text = string.Format("FUEL [ {0} / {1} ]", currentFuel, maxFuel);
        fuelBar.fillAmount = (float)currentFuel / (float)maxFuel;
    }
    private IEnumerator UpdateHpHUDCoroutine(int currentHP, int maxHP, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        hpLabel.text = string.Format("HP: [{0}/{1}]", currentHP, maxHP);
        hpBar.fillAmount = (float)currentHP / (float)maxHP;
    }    
}
