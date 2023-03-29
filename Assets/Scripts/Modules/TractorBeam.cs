using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeam : Module
{
    public new class InitializationData : Module.InitializationData
    {

    }
    public new class UsageData : Module.UsageData
    {

    }

    // #INSPECTOR
    [TitleGroup("BEAM SETTINGS")][SerializeField] int beamStrength;

    [TitleGroup("DEBUGGING")][SerializeField] private bool verboseLogging;
    [TitleGroup("DEBUGGING")][SerializeField] private bool verboseInitializationLogging;
    
    
    // #METHODS
    public override bool UseModule()
    {
        return true;
    }

}
