using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDEntry : MonoBehaviour
{
    public Sprite activeIcon { get; private set; }
    public Sprite inactiveIcon { get; private set; }
    public bool EntryIsActive
    {
        get
        {
            if (spriteElementUI.sprite == activeIcon)
                return true;
            else
                return false;
        }
    }
    public Module ReferenceToModuleOnPlayer { get; set; }


    // #FIELDS
    private GameObject selectorObjectUI;
    private Image spriteElementUI;
    private Text textElementUI;
    
    
    // #METHODS
    public void Init(Module.InitializationData iData)
    {
        selectorObjectUI = gameObject.GetComponentInChildren<RawImage>().gameObject;

        spriteElementUI = gameObject.GetComponentInChildren<Image>();

        textElementUI = gameObject.GetComponentInChildren<Text>();
        textElementUI.color = Color.white;

        activeIcon = iData.activeIcon;
                    
        inactiveIcon = iData.inactiveIcon;
        spriteElementUI.sprite = inactiveIcon;

        textElementUI.text = iData.capacityAmmo.ToString();
    }

    public void Refresh(int ammoAmount, bool active)
    {
        UpdateText(ammoAmount);
        ToggleIcon(active);
    }

    public void SetSelector(bool isSelected)
    {
        selectorObjectUI.SetActive(isSelected);
    }

    private void ToggleIcon(bool active)
    {
        if (active)
            spriteElementUI.sprite = activeIcon;
        else
            spriteElementUI.sprite = inactiveIcon;
    }

    private void UpdateText(int value)
    {
        textElementUI.text = value.ToString();
    }
}
