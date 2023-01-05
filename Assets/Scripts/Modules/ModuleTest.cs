using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleTest : Module
{
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private AmmunitionType ammoType = AmmunitionType.Batteries;
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private int displayCurrentAmmoInspector;
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private int cooldownCounter;

    public override bool UseModule()
    {
        throw new System.NotImplementedException();
    }
}
