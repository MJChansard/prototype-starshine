using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] lootInventory;

    public GameObject RequestLootDrop(Vector3 spawnLocation, bool forced = false)
    {
        if (forced || EligibleToDropLoot())
        {
            GameObject loot = CreateLootObject(spawnLocation);
            return loot;
        }
        else return null;
    }

    private bool EligibleToDropLoot() { return Random.Range(0, 10) > 4; }

    private GameObject CreateLootObject(Vector3 spawnLocation)
    {      
        int index = Random.Range(0, lootInventory.Length);
        GameObject droppedLoot = Instantiate(lootInventory[index], spawnLocation, Quaternion.Euler(-32.0f, -18.0f, -26.0f));

        droppedLoot.GetComponent<Rotator>().enabled = true;     

        return droppedLoot;
    }
}
