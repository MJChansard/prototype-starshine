using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : MonoBehaviour
{
    public Vector2Int delta;
    public int moveRate;

    // Start is called before the first frame update
    void Start()
    {
        if (this.gameObject.name == "Large Asteroid(Clone)")
        {
            delta = Vector2Int.down;
        }

        if (this.gameObject.name == "Small Asteroid(Clone)")
        {
            delta = Vector2Int.down + Vector2Int.down;
        }
    }

    public Vector2Int SetMovePatternUp(int distance)
    {
        for (int i = 0; i <= distance; i++)
        {
            delta += Vector2Int.up;
        }

        return delta;
    }

    public Vector2Int SetMovePatternDown(int distance)
    {
        for (int i = 0;  i <= distance; i++)
        {
            delta += Vector2Int.down;
        }

        return delta;
    }

    public Vector2Int SetMovePatternLeft(int distance)
    {
        for (int i = 0; i <= distance; i++)
        {
            delta += Vector2Int.left;
        }

        return delta;
    }

    public Vector2Int SetMovePatternRight(int distance)
    {
        for (int i = 0; i <= distance; i++)
        {
            delta += Vector2Int.right;
        }

        return delta;
    }
}
