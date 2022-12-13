using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;

public class Thruster : Module
{
    // #INSPECTOR    
    public class UsageData
    {
        public bool EligibleToMove { get; private set; }
        public Vector2Int DirectionToMove { get; private set; }
        public int NumberOfMoves { get; private set; }

        public UsageData(bool eligibleToMove, int numberOfMoves)
        {
            EligibleToMove = eligibleToMove;
            NumberOfMoves = numberOfMoves;
        }
    }

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
    public UsageData LatestUsageData { get; private set; }

    // #UNITY
    private void Awake()
    {
        currentDirection = Vector2Int.up;
        slowTicksRemaining = 0;
        hasMovedOnPlayerPhase = false;
    }

    // #METHODS
    public override bool UseModule()
    {        
        if (this.CanCurrentlyMove)
        {
            LatestUsageData = new Thruster.UsageData(CanCurrentlyMove, 1);
            return true;
        }
        else { return false; }
    
    }

    public override void AnimateModule(GridBlock gb)
    {
        throw new System.NotImplementedException();
    }
}