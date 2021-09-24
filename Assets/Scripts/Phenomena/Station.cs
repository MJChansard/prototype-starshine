using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Station : GridObject
{
    // INSPECTOR
    [TitleGroup("STATION SETTINGS")]
    [SerializeField] private bool overrideDefaultDockings = false;
    [ShowIf("overrideDefaultDockings")][SerializeField] private int inspectorNumberOfDockingsAllowed;
    
    [SerializeField] private bool canReplenish;
    [SerializeField] private int replenishTickDuration;
        

    public bool DockingAvailable { get { return numberOfDockingsAllowed > 0; } }


    private int numberOfDockingsAllowed = 1;
    private int dockingCounter = 1;
    private int replenishCounter;


    
    private void Awake()
    {
        if (overrideDefaultDockings)
        {
            numberOfDockingsAllowed = inspectorNumberOfDockingsAllowed;
            dockingCounter = inspectorNumberOfDockingsAllowed;
        }

        if (replenishTickDuration < 1)
        {
            replenishTickDuration = Random.Range(5, 11);
        }
    }


    // METHODS
    public void OnTickUpdate()
    {
        replenishCounter++;
        if (replenishCounter >= replenishTickDuration)
            replenishCounter = replenishTickDuration;
    }

    public bool Dock()
    {
        // bool indicates that Player successfully docked
        if (DockingAvailable)
        {
            dockingCounter--;
            return true;
        }    
        else
        {
            return false;
        }
    }
    public void AttemptReplenish()
    {
        if (canReplenish && !DockingAvailable)
        {
            replenishCounter = 0;
        }
    }




}
