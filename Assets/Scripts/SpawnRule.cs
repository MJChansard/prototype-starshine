using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class SpawnRule
{
    public enum SpawnRegion { Anywhere, Perimeter, Interior, Center }
    public SpawnRegion spawnRegion = SpawnRegion.Anywhere;

    //public enum Category { Hazard, Phenomena, Station };
    //public Category category;
    public GridObjectType spawnCategory;


    [ShowIf("forAvoidHazardPaths")] public bool avoidHazardPaths = false;
    [ShowIf("forRequiresOrientation")] public bool requiresOrientation = false;
    [ShowIf("forAvoidAdjacentToPlayer")] public bool avoidAdjacentToPlayer = false;
    [ShowIf("forAvoidShareSpawnLocation")] public bool avoidShareSpawnLocation = false;

    private bool forAvoidHazardPaths
    {
        get
        {
            if (spawnRegion == SpawnRegion.Perimeter)
                return true;
            else
                return false;

            /*
            if (spawnCategory == GridObjectType.Hazard)
                return true;
            else if (spawnCategory == GridObjectType.Loot)
                return true;
            else
                return false;
            */
        }
    }
    private bool forAvoidAdjacentToPlayer
    {
        get
        {
            if (spawnRegion == SpawnRegion.Interior)
                return true;
            else
                return false;

            /*
            if (spawnCategory == GridObjectType.Phenomena)
                return true;
            else if (spawnCategory == GridObjectType.Station)
                return true;
            else
                return false;
            */
        }
    }
    private bool forRequiresOrientation
    {
        get
        {
            if (spawnRegion == SpawnRegion.Perimeter)
                return true;
            else
                return false;
            /*
            if (spawnCategory == GridObjectType.Hazard)
                return true;
            else if (spawnCategory == GridObjectType.Loot)
                return true;
            else
                return false;
            */
        }
    }
    private bool forAvoidShareSpawnLocation
    {
        get
        {
            if (spawnRegion == SpawnRegion.Interior)
                return true;
            else
                return false;
        }
    }
}
