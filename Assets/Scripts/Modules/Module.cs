using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public abstract class Module : MonoBehaviour
{
    public class InitializationData
    {
        public Sprite availableIcon { get; set; }
        public Sprite useIcon { get; set; }
        public Sprite cooldownIcon { get; set; }

        public bool usesAmmo { get; set; }
        //public int capacityAmmo;
        public int cooldownDuration { get; set; }
    }

    public abstract class UsageData { }
  

    public enum AmmunitionType
    {
        None = 0,
        Rounds = 1,
        Missiles = 2,
        Batteries = 3
    }

    // #INSPECTOR
    [SerializeField] protected bool VerboseConsole;

    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] protected int cooldownCounter;
    [ShowIf("usesAmmo")][TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] protected AmmunitionType displayAmmoType;
    [ShowIf("usesAmmo")][TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] protected int displayCurrentAmmoInspector;

    [TitleGroup("GENERAL SETTINGS")][SerializeField] protected string moduleName;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] protected bool usesAmmo;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] protected bool hasAnimation;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] protected int cooldownDuration;

    [TitleGroup("ASSETS")][SerializeField] protected Sprite availableIcon;
    [TitleGroup("ASSETS")][SerializeField] protected Sprite useIcon;
    [TitleGroup("ASSETS")][SerializeField] protected Sprite cooldownIcon;

    [ShowIf("usesAmmo")][TitleGroup("AMMUNITION SETTINGS")][SerializeField] protected AmmunitionType ammoType;
    [ShowIf("usesAmmo")][TitleGroup("AMMUNITION SETTINGS")][SerializeField] protected int capacityAmmo;
    [ShowIf("usesAmmo")][TitleGroup("AMMUNITION SETTINGS")][SerializeField] protected int startAmmo;
    [ShowIf("usesAmmo")][TitleGroup("AMMUNITION SETTINGS")][SerializeField] protected int usageAmmoCost;
    

    // #PROPERTIES
    public InitializationData InitData
    {
        get
        {
            return new InitializationData()
            {
                availableIcon = this.availableIcon,
                useIcon = this.useIcon,
                cooldownIcon = this.cooldownIcon
            };
        }
    }
    public bool HasAnimation { get { return hasAnimation; } }

    // #METHODS
    public abstract bool UseModule();
}

// END OF FILE