using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Module
{
    public new class UsageData : Module.UsageData
    {


        

    }

    // #INSPECTOR

    // #FIELDS

    // #UNITY
    private void Awake()
    {
        displayCurrentAmmoInspector = 0;
        cooldownCounter = 0;
    }

    // #METHODS
    public override bool UseModule()
    {
        throw new System.NotImplementedException();
    }

    public override void AnimateModule(GridBlock gb)
    {
        throw new System.NotImplementedException();
    }
}
