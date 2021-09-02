using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ContactFuel : MonoBehaviour
{
    public int fuelAmount;
    public bool randomizeNumberRefuelsRemaining;
    [ShowIf("@!randomizeNumberRefuelsRemaining")]
    public int numberOfRefuelsRemaining;

    private void Start()
    {
        if (randomizeNumberRefuelsRemaining)
            numberOfRefuelsRemaining = Random.Range(1, 5);
    }       

}
