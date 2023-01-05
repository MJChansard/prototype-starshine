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
    [SerializeField] private GameObject RefToHpNodeGameObject;
    [SerializeField] private Image batteryBar;
    [SerializeField] private Text batteryLabel;
    [SerializeField] private PlayerHUDEntry playerHUDEntryPrefab;

    // Color hex for disabled node 


    //  #FIELDS
    private GameObject modulesGroupHUD;
    private PlayerHUDEntry[] entryArrayHUD;// = new PlayerHUDEntry[6];
    private int entryArrayHUDIndex;
    private Image[] hpNodes;
    private Color activeNode;       //#00FFF0
    private Color inactiveNode;     //#383838

    //  References
    private InputManager inputM;

    // #DELEGATES
    public System.Action OnPlayerAdvance;
    public System.Action OnPlayerHUDModuleActivate;

    private void Awake()
    {
        entryArrayHUDIndex = 0;

        activeNode = new Color(0, 255, 240);
        inactiveNode = new Color(56, 56, 56);


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


        if (RefToHpNodeGameObject == null)
            Debug.Log("Please assign reference to UI HP Node GameObject.");
        else
        {
            for (int i = 0; i < RefToHpNodeGameObject.transform.childCount; i++)
            {
                hpNodes[i] = RefToHpNodeGameObject.transform.GetChild(i).GetComponent<Image>();
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
            inputM.ActivateModuleButtonPressed += OnModuleUse;
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
                entry.Init(modulesForUI[i].InitData);
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

        //UpdateFuelHUD(0, maxFuelAmount, 0.0f);
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
    private void OnModuleUse()
    {
        entryArrayHUD[entryArrayHUDIndex].OnModuleUse();
    }

    public void RefreshHUDEntry(int newAmmoAmount, bool currentlySelected)
    {
        entryArrayHUD[entryArrayHUDIndex].Refresh(newAmmoAmount, currentlySelected);
    }


    public void UpdateBatteryHUD(int percentAmount, float animateDelay)
    {
        StartCoroutine(UpdateBatteryHUDCoroutine(percentAmount, animateDelay));
    }
    public void UpdateHpHUD(int currentPlayerHP, int maxPlayerHP, float animateDelay)
    {
        StartCoroutine(UpdateHpHUDCoroutine(currentPlayerHP, animateDelay));
    }


    private IEnumerator UpdateBatteryHUDCoroutine(int percentAmount, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        batteryBar.fillAmount += (float)percentAmount;
    }
    private IEnumerator UpdateHpHUDCoroutine(int currentHP, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        int maxHP = RefToHpNodeGameObject.transform.childCount;
        //hpLabel.text = string.Format("HP: [{0}/{1}]", currentHP, maxHP);
        //hpBar.fillAmount = (float)currentHP / (float)maxHP;

        for (int i =  maxHP - 1; i > currentHP - 1; i--)
        {
            hpNodes[i].color = inactiveNode;
        }
    }    
}
