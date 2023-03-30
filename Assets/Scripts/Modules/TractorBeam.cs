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
        public int BeamStrengh { get; set; }
        public int BeamRange { get; set; }
        public Vector2Int BeamDirection { get; set; }
    }

    // #INSPECTOR
    [TitleGroup("BEAM SETTINGS")][SerializeField] int beamStrength;
    [TitleGroup("BEAM SETTINGS")][SerializeField] int beamRange;

    [TitleGroup("DEBUGGING")][SerializeField] private bool verboseLogging;
    [TitleGroup("DEBUGGING")][SerializeField] private bool verboseInitializationLogging;

    // #PROPERTIES
    public UsageData LatestUsageData { get; private set; }

    // #FIELDS
    int currentAmmo;

    private void Awake()
    {
        currentAmmo = startAmmo;
    }

    // #METHODS
    public override bool UseModule()
    {
        bool sufficientResources = ConsumeResource(ammoType);

        LatestUsageData = new TractorBeam.UsageData()
        {
            BeamStrengh = beamStrength,
            BeamRange = beamRange
            //BeamDirection will be set by Player.cs
        };
         
        return sufficientResources;
    }

    private bool ConsumeResource(AmmunitionType r)
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            return true;
        }
        else
        {
            return false;
        }
    }
}
