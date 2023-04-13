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
        //bool sufficientResources = ConsumeResource(ammoType);
        
        LatestUsageData = new TractorBeam.UsageData()
        {
            BeamStrengh = beamStrength,
            BeamRange = beamRange
            //BeamDirection will be set by Player.cs
        };

        //return sufficientResources;       
        return true;
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

    public IEnumerator AnimateCoroutine(Transform player)
    {
        lr.SetPosition(0, player.position);

        Vector3 direction = (TargetTransform.position - player.position).normalized;
        Vector3 animationEndPosition = TargetTransform.position + direction;            // Tractor beam target's destination after movement     

        float startTime = Time.time;
        float percentTraveled = 0.0f;
        float distance = Vector3.Distance(TargetTransform.position, player.position);

        while (percentTraveled <= 1.0f)
        {
            float traveled = (Time.time - startTime) * 1.0f;
            percentTraveled = traveled / distance;
            lr.SetPosition(1, Vector3.Lerp(lr.GetPosition(0), animationEndPosition, Mathf.SmoothStep(0, 1, percentTraveled)));
            yield return null;
        }

        lr.SetPosition(1, lr.GetPosition(0));
    }
}