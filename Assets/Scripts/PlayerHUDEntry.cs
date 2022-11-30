using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDEntry : MonoBehaviour
{
    public Sprite availableIcon { get; private set; }
    public Sprite useIcon { get; private set; }
    public Sprite cooldownIcon { get; private set; }
    public bool EntryIsActive
    {
        get
        {
            if (spriteElementUI.sprite == availableIcon)
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

        availableIcon = iData.availableIcon;
        useIcon = iData.useIcon;
        cooldownIcon = iData.cooldownIcon;

        spriteElementUI.sprite = availableIcon;
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
            spriteElementUI.sprite = availableIcon;
        else
            spriteElementUI.sprite = useIcon;
    }

    private void UpdateText(int value)
    {
        textElementUI.text = value.ToString();
    }
}
