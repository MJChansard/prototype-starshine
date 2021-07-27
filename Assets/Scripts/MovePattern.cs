using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : MonoBehaviour
{
    
    [SerializeField] private bool AlwaysMoveOnSpawn = true;

    [SerializeField] private int waitTicksPerMove;
    public int ticksUntilNextMove;

    public Vector3 DirectionInWorld
    {
        get { return new Vector3(delta.x, delta.y, 0.0f); }
    }

    public Vector2Int DirectionOnGrid
    {
        get { return delta; }
    }

    private Vector2Int delta;

    private void Awake()
    {
        ticksUntilNextMove = (AlwaysMoveOnSpawn) ? 1 : waitTicksPerMove;        
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

    /*  Careful with calls to this method as timing has a great impact
     *  on GameObjectManager.OnTickUpdate() and 
     *  GameObjectManager.MoveObjectsForTick()
     */
    public void OnTickUpdate()
    {
        if(ticksUntilNextMove == 0)
        {
            ticksUntilNextMove = waitTicksPerMove;
        }
        ticksUntilNextMove -= 1;
    }

    /*
    public bool CanMoveThisTurn()
    {
        return ticksUntilNextMove == 0;
    }
    */

    public bool CanMoveThisTurn
    {
        get { return ticksUntilNextMove == 0; }
 
    }

    public void ApplyMoveDelay(int delayValue)
    {
        ticksUntilNextMove += delayValue;
    }
}
