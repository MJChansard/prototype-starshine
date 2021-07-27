using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phenomena : GridObject
{

    [HideInInspector] public bool DoesDamage;
    [HideInInspector] public bool DoesRepair;
    [HideInInspector] public bool DoesSupply;
    [HideInInspector] public bool DoesFuel;
    [HideInInspector] public bool DoesLeak;

    private void Start()
    {
        DoesDamage = this.gameObject.TryGetComponent<ContactDamage>(out ContactDamage _);
        DoesRepair = this.gameObject.TryGetComponent<ContactRepair>(out ContactRepair _);
        DoesSupply = this.gameObject.TryGetComponent<ContactSupply>(out ContactSupply _);
        DoesFuel = this.gameObject.TryGetComponent<ContactFuel>(out ContactFuel _);
        DoesLeak = this.gameObject.TryGetComponent<ContactLeak>(out ContactLeak _);
    }



}
