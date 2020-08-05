using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] lootInventory;

    public GameObject RequestLootDrop(Vector3 spawnLocation, bool forced = false)
    {
        if (EligibleToDropLoot(forced))
        {
            //return DropLoot();
            GameObject loot = DropLoot(spawnLocation);
            //Instantiate(loot, spawnLocation, Quaternion.Euler(-32.0f, -18.0f, -26.0f));
            return loot;
        }
        else return null;
    }

    private bool EligibleToDropLoot(bool forced = false)
    {
        if (forced == true) return true;
        else
        {
            int seed = Random.Range(0, 10);

            if (seed > 4) return true;
            else return false;
        }
    }

    private GameObject DropLoot(Vector3 spawnLocation)
    {
        /*  IDEA
         * 
         *  Add a Constructor to LootItem.cs that initializes name and ammo value properties in this method.
         *  Then remove the LootItem component from the Prefab.
         *  Use AddComponent() to attach and initalize the LootItem in this method.
         */
        
        int index = Random.Range(0, lootInventory.Length);
        GameObject droppedLoot = Instantiate(lootInventory[index], spawnLocation, Quaternion.Euler(-32.0f, -18.0f, -26.0f));
        return droppedLoot;
    }
}
