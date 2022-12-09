using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Module
{
    // #INSPECTOR
    [SerializeField] private bool VerboseConsole;

    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private AmmunitionType ammoType = AmmunitionType.Batteries;
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private int displayCurrentAmmoInspector;
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private int cooldownCounter;

    [TitleGroup("ASSETS")][SerializeField] private Sprite availableIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite useIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite cooldownIcon;

    [TitleGroup("SHIELD SETTINGS")][SerializeField] private int capacityAmmo;
    [TitleGroup("SHIELD SETTINGS")][SerializeField] private int startAmmo;
    [TitleGroup("SHIELD SETTINGS")][SerializeField] private int usageAmmoCost;



    // # Fields

    private void Awake()
    {
        displayCurrentAmmoInspector = 0;
        cooldownCounter = 0;
    }

    public override bool UseModule(out UsageData data)
    {
        throw new System.NotImplementedException();
    }

    public override void AnimateModule(GridBlock gb)
    {
        throw new System.NotImplementedException();
    }
}
