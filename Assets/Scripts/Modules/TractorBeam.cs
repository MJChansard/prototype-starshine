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
        public LineRenderer refToRenderer { get; set; }
    }

    // #INSPECTOR
    [TitleGroup("BEAM SETTINGS")][SerializeField] int beamStrength;
    [TitleGroup("BEAM SETTINGS")][SerializeField] int beamRange;

    [TitleGroup("DEBUGGING")][SerializeField] private bool verboseLogging;
    [TitleGroup("DEBUGGING")][SerializeField] private bool verboseInitializationLogging;

    // #PROPERTIES
    public UsageData LatestUsageData { get; private set; }
    public Transform TargetTransform { get; set; }

    // #FIELDS
    private int currentAmmo;
    private LineRenderer lr;

    private void Awake()
    {
        currentAmmo = startAmmo;
        lr = GetComponent<LineRenderer>();
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

    public IEnumerator AnimateCoroutine(Transform player, Transform target)
    {

        lr.SetPosition(0, player.position);
        lr.SetPosition(1, target.position);
        
        yield return new WaitForFixedUpdate();

    }
}
