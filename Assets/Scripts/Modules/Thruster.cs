using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;

public class Thruster : Module
{
    // #INSPECTOR    
    public new class UsageData : Module.UsageData
    {
        public bool EligibleToMove { get; private set; }
        public Vector2Int DirectionToMove { get; private set; }
        public int NumberOfMoves { get; private set; }

        public override string ToString()
        {
            string result = string.Format("Eligible To Move: {0}\nDirection To Move: {1}\nNumberOfMoves: {2}", EligibleToMove, DirectionToMove, NumberOfMoves);
            return result;
        }

        public UsageData(bool eligibleToMove, Vector2Int direction, int numberOfMoves)
        {
            EligibleToMove = eligibleToMove;
            DirectionToMove = direction;
            NumberOfMoves = numberOfMoves;
        }
    }

    // #FIELDS
    private int slowTicksRemaining;
    ParticleSystem ps;


    // #PROPERTIES
    public bool CanCurrentlyMove
    {
        get
        {
            if (slowTicksRemaining == 0)
                return true;
            else
                return false;
        }
    }
    public Vector2Int CurrentDirectionFacing { get; set; }
    public UsageData LatestUsageData { get; private set; }

    // #UNITY
    private void Awake()
    {
        CurrentDirectionFacing = Vector2Int.up;
        slowTicksRemaining = 0;
        ps = GetComponent<ParticleSystem>();
    }


    // #METHODS
    public override bool UseModule()
    {        
        if (this.CanCurrentlyMove)
        {
            LatestUsageData = new Thruster.UsageData(true, CurrentDirectionFacing, 1);
            StartCoroutine(AnimateCoroutine());
            return true;
        }
        else { return false; }
    }

    private IEnumerator AnimateCoroutine()
    {
        ps.Play();
        yield return new WaitForSeconds(1.0f);
        ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}