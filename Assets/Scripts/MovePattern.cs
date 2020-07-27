using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : MonoBehaviour
{
    public Vector2Int delta;
    public int moveRate;

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
}
