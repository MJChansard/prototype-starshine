using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleThruster : MonoBehaviour
{
    // #INSPECTOR
    [SerializeField] private bool VerboseConsole;

    [TitleGroup("GENERAL SETTINGS")][SerializeField] private string moduleName;    
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool hasAnimation;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private int cooldownDuration;

    [TitleGroup("ASSETS")][SerializeField] private Sprite availableIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite useIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite cooldownIcon;  
    
    [TitleGroup("AMMUNITION SETTINGS")][SerializeField] private int capacityAmmo;
    [TitleGroup("AMMUNITION SETTINGS")][SerializeField] private int startAmmo;
    [TitleGroup("AMMUNITION SETTINGS")][SerializeField] private int usageAmmoCost;
    
    [TitleGroup("CURRENT STATUS")][DisplayAsString] private int displayCurrentAmmoInspector;
    [TitleGroup("CURRENT STATUS")][DisplayAsString] private int cooldownCounter;

    // #FIELDS
    private Vector2Int direction;

    // #PROPERTIES

    // #METHODS

    public void UseModule()
    {

    }
}