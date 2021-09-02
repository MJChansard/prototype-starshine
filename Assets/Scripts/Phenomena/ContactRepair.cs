using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ContactRepair : MonoBehaviour
{
    public int repairAmount;
    [InfoBox("Selecting Randomize will override any value placed in the Repairs Remaining property.")]
    public bool randomizeNumberOfRepairsRemaining;
    [ShowIf("@!randomizeNumberOfRepairsRemaining")]
    public int repairsRemaining;

    private void Start()
    {
        if (randomizeNumberOfRepairsRemaining)
            repairsRemaining = Random.Range(1, 5);
    }

    public void ConsumeRepair()
    {
        repairsRemaining--;
    }

}
