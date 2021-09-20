using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : MonoBehaviour
{

    [SerializeField] private bool AlwaysMoveOnSpawn = true;
    [SerializeField] private int waitTicksPerMove;
    public int ticksUntilNextMove;


    public Vector2Int DirectionOnGrid
    {
        get { return delta; }
    }
    private Vector2Int delta;

    // PROPERTIES: Slow
    public bool EligibleToSlow { get { return eligibleSlow; } }
    public bool CanMoveThisTurn
    {
        get { return ticksUntilNextMove == 0 && slowTicksRemaining == 0; }
    }
    private bool eligibleSlow = true;
    private int slowTicksRemaining;
    

    private void Awake()
    {
        ticksUntilNextMove = (AlwaysMoveOnSpawn) ? 1 : waitTicksPerMove;        
    }
    public void OnTickUpdate()
    {
        /*  Careful with calls to this method as timing has a great impact
         *  on GameObjectManager.OnTickUpdate() and 
         *  GameObjectManager.MoveObjectsForTick()
         */
        if (ticksUntilNextMove == 0)
        {
            ticksUntilNextMove = waitTicksPerMove;

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
    
    
    // METHODS: SET DIRECTION
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
        Debug.LogFormat("Delaying Movement of {0} by {1} turns.", this.gameObject.name, slowTickDuration.ToString());

        if (ticksUntilNextMove > 0)
            slowTicksRemaining = ticksUntilNextMove + slowTickDuration;
        else
            slowTicksRemaining = waitTicksPerMove + slowTickDuration;

        eligibleSlow = false;
    }
}
