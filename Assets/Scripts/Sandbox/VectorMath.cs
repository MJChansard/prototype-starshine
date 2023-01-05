using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Net.NetworkInformation;

public class VectorMath : MonoBehaviour
{
    void Awake()
    {
        Vector2Int origin = new Vector2Int(0, 0);
        Vector2Int direction = new Vector2Int(-1, 0);
        int turn1 = 1;
        int turn2 = 2;

        Debug.LogFormat("Turn 0 Origin: {0}.  Turn 0 Direction: {1}", origin, direction);
        
        origin = (origin + direction) * turn1;
        Debug.LogFormat("Turn 1 Origin: {0}", origin);

        origin = (origin + direction) * turn2;
        Debug.LogFormat("Turn 2 Origin: {0}", origin);
    }

    public Vector2Int origin;
    public Vector2Int direction;
    public int cells;

    [DisplayAsString] public Vector2Int output;

    [Button("ApplyTurn")]
    private void ApplyTurn()
    {
        output = (origin + direction) * cells;
    }
}
