using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;

public class Thruster : Module
{
    // #INSPECTOR
    [SerializeField] private bool VerboseConsole;
    
    // #FIELDS
    private Vector2Int currentDirection;
    private int slowTicksRemaining;
    private bool hasMovedOnPlayerPhase;

    // #PROPERTIES
    public bool CanCurrentlyMove
    {
        get
        {
            if (slowTicksRemaining == 0 && !hasMovedOnPlayerPhase)
                return true;
            else
                return false;
        }
    }

    // #UNITY
    private void Awake()
    {
        currentDirection = Vector2Int.up;
        slowTicksRemaining = 0;
        hasMovedOnPlayerPhase = false;
    }

    // #METHODS
    public override bool UseModule(out Module.UsageData data)
    {
        data = new Module.UsageData()
        {
            doesMove = true,
            doesDamage = false,
            doesPenetrate = false,
            doesPlaceObjectInWorld = false
        };
        
        if (this.CanCurrentlyMove)
        {
            return true;
        }
        else { return false; }
    
    }

    public override void AnimateModule(GridBlock gb)
    {
        throw new System.NotImplementedException();
    }
}