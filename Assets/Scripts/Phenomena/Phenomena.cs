using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phenomena : GridObject
{
    public Sprite IconForLevelEditor;
    [HideInInspector] public bool DoesDamage;
  
    [HideInInspector] public bool DoesRepair;
    public int RepairAmount
    {
        get
        {
            if (DoesRepair)
                return refRepair.repairAmount;
            else return 0;
        }
    }
    private ContactRepair refRepair;

    
    [HideInInspector] public bool DoesSupply;
    [HideInInspector] public bool DoesFuel;
    [HideInInspector] public bool DoesLeak;

    private void Awake()
    {
        DoesDamage = this.gameObject.TryGetComponent<ContactDamage>(out ContactDamage _);
        
        DoesRepair = this.gameObject.TryGetComponent<ContactRepair>(out ContactRepair cr);
        refRepair = cr;

        DoesSupply = this.gameObject.TryGetComponent<ContactSupply>(out ContactSupply _);
        DoesFuel = this.gameObject.TryGetComponent<ContactFuel>(out ContactFuel _);
        DoesLeak = this.gameObject.TryGetComponent<ContactLeak>(out ContactLeak _);
    }
}