using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : MonoBehaviour
{
    public Vector2Int delta;
    public int moveRate;
    // ticksPerMove;

    public Vector2Int SetMovePatternUp(int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            delta += Vector2Int.up;
        }

        return delta;

        // Store velocity float
        // If velocity == 0.5, add value
        // When value hits threshold PerformMove()
        // Subtract 1 from value to "reset"
    }

    public Vector2Int SetMovePatternDown(int distance)
    {
        for (int i = 0;  i < distance; i++)
        {
            delta += Vector2Int.down;
        }

        return delta;
    }

    public Vector2Int SetMovePatternLeft(int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            delta += Vector2Int.left;
        }

        return delta;
    }

    public Vector2Int SetMovePatternRight(int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            delta += Vector2Int.right;
        }

        return delta;
    }
}
