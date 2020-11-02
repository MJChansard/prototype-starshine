using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : MonoBehaviour
{
    public Vector2Int delta;    
    [SerializeField] private bool AlwaysMoveOnSpawn = true;

    [SerializeField] private int waitTicksPerMove = 1;
    private int ticksUntilNextMove;

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



    public void OnTickUpdate()
    {
        if(ticksUntilNextMove == 0)
        {
            ticksUntilNextMove = waitTicksPerMove;
        }
        ticksUntilNextMove -= 1;
    }

    public bool CanMoveThisTurn()
    {
        return ticksUntilNextMove == 0;
    }
}
