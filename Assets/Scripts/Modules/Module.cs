using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class Module : MonoBehaviour
{
    public class InitializationData
    {
        public Sprite availableIcon;
        public Sprite useIcon;
        public Sprite cooldownIcon;

        public bool usesAmmo;
        public int capacityAmmo;
        public int cooldownDuration;
    }

    public class UsageData
    {
        public bool doesMove;
        
        public bool doesDamage;
        public bool doesPenetrate;

        public int baseDamage;
        public bool dynamicDamage;
        public float damageMultiplier;
        
        public bool doesPlaceObjectInWorld;
        public GameObject objectToPlaceInWorld;

        public int newAmmoAmount;
    }
    
    public enum AmmunitionType
    {
        None = 0,
        Rounds = 1,
        Missiles = 2,
        Batteries = 3
    }

    // #INSPECTOR
    [SerializeField] private bool VerboseConsole;

    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private AmmunitionType displayAmmoType;
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private int displayCurrentAmmoInspector;
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private int cooldownCounter;

    [TitleGroup("GENERAL SETTINGS")][SerializeField] private string moduleName;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool hasAnimation;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private int cooldownDuration;

    [TitleGroup("ASSETS")][SerializeField] private Sprite availableIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite useIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite cooldownIcon;

    [TitleGroup("SHIELD SETTINGS")][SerializeField] private int cooldownLength;

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

    // #METHODS
    public abstract bool UseModule(out UsageData data);
    public abstract void AnimateModule(GridBlock gb);
    
}

// END OF FILE