using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "SpawnStep", menuName = "Scriptable Objects/Spawn Step", order = 1)]
public class SpawnRecord : ScriptableObject
{
    [AssetSelector(Paths = "Assets/Prefabs/GridObjects")]
    public GridObject GridObject;
    public Vector2Int GridLocation;
    public Vector3 WorldLocation
    {
        get
        {
            Vector3 result = new Vector3();
            result.Set(GridLocation.x, GridLocation.y, 0.0f);
            return result;
        }
    }
    public GridBorder Border;



    // METHODS
    public static SpawnRecord CreateSpawnRecord()
    {
        SpawnRecord result = ScriptableObject.CreateInstance<SpawnRecord>();
        return result;
    }

    public void Init(GridObject type, Vector2Int location)
    {
        this.GridObject = type;
        this.GridLocation = location;
    }

}