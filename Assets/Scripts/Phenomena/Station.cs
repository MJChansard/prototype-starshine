using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Station : GridObject
{
    // INSPECTOR
    //[TitleGroup("DISPLAY")]
    [TitleGroup("DISPLAY", alignment: TitleAlignments.Split)][ReadOnly] public int displayDockingCounter;
    [TitleGroup("DISPLAY", alignment: TitleAlignments.Split)][ReadOnly] public int displayReplenishCounter;

    [TitleGroup("STATION SETTINGS")]
    [SerializeField] private bool overrideDefaultSettings = false;
    [ShowIf("overrideDefaultSettings")][SerializeField] private bool limitNumberOfDockings = false;
    [ShowIf("odinShowDockingSettings")][SerializeField] private int numberOfDockingsAllowed;
    [ShowIf("overrideDefaultSettings")][SerializeField] private bool canReplenish;
    [ShowIf("overrideDefaultSettings")][SerializeField] private int replenishTickDuration;

    [TitleGroup("STATION APPEARANCE")]
    [SerializeField] private Material dockAvailableMaterial;
    [SerializeField] private Material dockNotAvailableMaterial;

    private bool odinShowDockingSettings()
    {
        return (overrideDefaultSettings == true && limitNumberOfDockings == true);
    }


    public bool DockingAvailable
    { 
        get
        {
            return (limitNumberOfDockings == false || dockingCounter > 0) && replenishCounter >= replenishTickDuration;
        }
    }


    private int dockingCounter;
    private int replenishCounter;

    //private GameObject[] dockingLights;
    private Renderer dockLightA;
    private Renderer dockLightB;
    private Renderer dockLightC;
    private Rotator r;


    private void Awake()
    {
        //dockingLights = new GameObject[3];

        /*
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "DockLightA")
            {
                dockingLights[0] = child.gameObject;
                dockingLights[0].GetComponent<Renderer>().material = dockAvailableLight;
            }
            else if (child.name == "DockLightB")
            {
                dockingLights[1] = child.gameObject;
                dockingLights[1].GetComponent<Renderer>().material = dockAvailableLight;
            }
            else if (child.name == "DockLightC")
            {
                dockingLights[2] = child.gameObject;
                dockingLights[2].GetComponent<Renderer>().material = dockAvailableLight;
            }
        }
        */

        // Duplicate of above for loop but different implementation
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "DockLightA")
            {
                dockLightA = child.GetComponent<Renderer>();
                dockLightA.material = dockAvailableMaterial;
            }
            else if (child.name == "DockLightB")
            {
                dockLightB = child.GetComponent<Renderer>();
                dockLightB.material = dockAvailableMaterial;
            }
            else if (child.name == "DockLightC")
            {
                dockLightC = child.GetComponent<Renderer>();
                dockLightC.material = dockAvailableMaterial;
            }
        }

        if (overrideDefaultSettings)
        {
            dockingCounter = numberOfDockingsAllowed;
        }
        else
        {
            numberOfDockingsAllowed = Random.Range(1, 4);
            canReplenish = true;
            replenishTickDuration = Random.Range(1, 6);
        }

        dockingCounter = numberOfDockingsAllowed;
        replenishCounter = replenishTickDuration;

        displayDockingCounter = dockingCounter;
        displayReplenishCounter = replenishCounter;

        r = gameObject.GetComponentInChildren<Rotator>();
    }


    // METHODS
    public void OnTickUpdate()
    {
        replenishCounter++;
        if (replenishCounter >= replenishTickDuration)
        {
            replenishCounter = replenishTickDuration;
        }

        displayDockingCounter = dockingCounter;
        displayReplenishCounter = replenishCounter;

        
        if (DockingAvailable)
        {
            dockLightA.material = dockAvailableMaterial;
            dockLightB.material = dockAvailableMaterial;
            dockLightC.material = dockAvailableMaterial;

            r.enabled = true;
        }
        else
        {
            dockLightA.material = dockNotAvailableMaterial;
            dockLightB.material = dockNotAvailableMaterial;
            dockLightC.material = dockNotAvailableMaterial;

            r.enabled = false;
        }
    }

    public bool Dock()
    {
        // bool indicates that Player successfully docked
        Debug.LogFormat("Attempting docking.  Dock avilable: {0}.", DockingAvailable);
        if (DockingAvailable)
        {
            dockingCounter--;
            Debug.LogFormat("Current value [dockingCounter]: {0}", dockingCounter.ToString());
            return true;
        }    
        else
        {
            return false;
        }
    }
    public void BeginReplenish()
    {
        if (canReplenish)
        {
            replenishCounter = -1;
        }
    }




}
