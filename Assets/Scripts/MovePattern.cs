using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePattern : MonoBehaviour
{
    public Vector2Int delta;

    // Start is called before the first frame update
    void Start()
    {
        if (this.gameObject.name == "Asteroid(Clone)")
        {
            delta = Vector2Int.down;
        }
    }
}
