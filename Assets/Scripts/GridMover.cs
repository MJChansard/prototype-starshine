using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GridMover : MonoBehaviour
{

    [System.Serializable] class Rules
    {
        [SerializeField] bool onTick;
        [SerializeField] bool onActivateFuncModule;
        [SerializeField] bool onIdle;

        public Rules()
        {
            onTick = false;
            onActivateFuncModule = false;
            onIdle = false;
        }
    }

    [System.Serializable] class Rate
    {
        [SerializeField] int gridBlockPerTick;
        [SerializeField] int gridBlockPerTurn;
        [SerializeField] int gridBlockPerLaunch;
        [SerializeField] int gridBlockPerIdle;

        public Rate()
        {
            gridBlockPerTick = 1;
            gridBlockPerTurn = 1;
            gridBlockPerLaunch = 1;
            gridBlockPerIdle = 1;
        }
    }

    //  #INSPECTOR
    [BoxGroup("MOVEMENT PROPERTIES", centerLabel: true)]
    //    [TitleGroup("MOVEMENT PROPERTIES/DATA")] [SerializeField] Rules rules = new Rules();
    //    [TitleGroup("MOVEMENT PROPERTIES/DATA")] [SerializeField] Rate rate = new Rate();
    [SerializeField] bool AlwaysMoveOnSpawn = true;
    [TitleGroup("MOVEMENT PROPERTIES/RULES")] [ShowInInspector] public bool onTick { get; private set; } = true;
    [TitleGroup("MOVEMENT PROPERTIES/RULES")] [ShowInInspector] public bool onActivateFunctionalModule { get; private set; } = true;
    [TitleGroup("MOVEMENT PROPERTIES/RULES")] [ShowInInspector] public bool onIdle { get; private set; } = true;
    [TitleGroup("MOVEMENT PROPERTIES/RULES")] [ShowInInspector] public bool canBeSlow { get; private set; } = true;

    [TitleGroup("MOVEMENT PROPERTIES/RATE")]
    [InfoBox("All rates of movement are measured in GridBlocks.")]
    [TitleGroup("MOVEMENT PROPERTIES/RATE")] [ShowInInspector] public int forTick { get; private set; } = 1;
    [TitleGroup("MOVEMENT PROPERTIES/RATE")] [ShowInInspector] public int forTurn { get; private set; } = 1;
    [TitleGroup("MOVEMENT PROPERTIES/RATE")] [ShowInInspector] public int forLaunch { get; private set; } = 1;
    [TitleGroup("MOVEMENT PROPERTIES/RATE")] [ShowInInspector] public int forIdle { get; private set; } = 1;

    
    //  #PROPERTIES
    public Vector2Int DirectionOnGrid { get { return delta; } }
    public bool IsSlowed { get { return slowTicksRemaining == 0; } }    
    public bool CanMoveThisTurn    // #Needs to be drastically reworked
    {
        //get { return turnsUntilNextMove == 0 && slowTicksRemaining == 0; }
        get
        {
            if (slowTicksRemaining > 0)
                return false;
            
            else return true;
        }
    }


    //  #FIELDS
    Vector2Int delta;
    int slowTicksRemaining;

    //  #METHODS
    public void OnTickUpdate()
    {
        if (slowTicksRemaining > 0)
            slowTicksRemaining--;

        //if (slowTicksRemaining == 0)
            //eligibleSlow = true;
    }
 
    public Vector2Int SetMovePatternUp()
    {
        delta = Vector2Int.up;
        return delta;
    }
    public Vector2Int SetMovePatternDown()
    {
        delta = Vector2Int.down;
        return delta;
    }
    public Vector2Int SetMovePatternLeft()
    {
        delta = Vector2Int.left;
        return delta;
    }
    public Vector2Int SetMovePatternRight()
    {
        delta = Vector2Int.right;
        return delta;
    }
    public void SetMovePattern(Vector2Int directionToMove)
    {
        delta = directionToMove;
    }


    // METHODS: ContactSlow   
    public void ApplySlow(int slowTickDuration)
    {
        Debug.LogFormat("Delaying Movement of {0} by {1} ticks.", this.gameObject.name, slowTickDuration.ToString());

        slowTicksRemaining = slowTickDuration;
    }
}

/*
public void OnTickUpdate()
{
    /*  Careful with calls to this method as timing has a great impact
     *  on GameObjectManager.OnTickUpdate() and 
     *  GameObjectManager.MoveObjectsForTick()
     *
    
if (movesPerTurn > 0)
        movesPerTurn--;
    /*
    if (turnsUntilNextMove == 0)
    {
        turnsUntilNextMove = waitTicksPerMove;

    }
    ticksUntilNextMove -= 1;

    if (slowTicksRemaining > 0)
    {
        slowTicksRemaining--;
    }
    else
    {
        slowTicksRemaining = 0;
        eligibleSlow = true;
    }   
}
*/
/*
void NewTurn()
{
    movesPerTurn = 1;
}
*/
/*
 * public void ApplySlow(int slowTickDuration)
    {
        Debug.LogFormat("Delaying Movement of {0} by {1} turns.", this.gameObject.name, slowTickDuration.ToString());

        if (movesPerTurn > 0)
            slowTicksRemaining = movesPerTurn + slowTickDuration;
        else
            slowTicksRemaining = slowTickDuration;

        eligibleSlow = false;
    }
*/