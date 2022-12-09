using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Module;

public class ThrusterModulePart : MonoBehaviour
{
    // #INSPECTOR
    [SerializeField] private bool VerboseConsole;

    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private Vector2Int DirectionFacing;
    [TitleGroup("CURRENT STATUS")][SerializeField][DisplayAsString] private int SlowTurnsRemaining;

    [TitleGroup("GENERAL SETTINGS")][SerializeField] private string moduleName;
    [TitleGroup("GENERAL SETTINGS")][SerializeField] private bool hasAnimation;
    
    [TitleGroup("ASSETS")][SerializeField] private Sprite availableIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite useIcon;
    [TitleGroup("ASSETS")][SerializeField] private Sprite cooldownIcon;


    // #FIELDS
    private bool hasMovedOnPlayerPhase;

    // #PROPERTIES
    public bool CanCurrentlyMove
    {
        get
        {
            if (SlowTurnsRemaining == 0 && !hasMovedOnPlayerPhase)
                return true;
            else
                return false;
        }
    }

    // #UNITY
    private void Awake()
    {
        DirectionFacing = Vector2Int.up;
        SlowTurnsRemaining = 0;
        hasMovedOnPlayerPhase = false;
    }

    // #METHODS
    public bool UseModule(out ModuleUseData data)
    {
        data = new ModuleUseData(damage: false, move: true, shield: false);

        if (CanCurrentlyMove)
        {
            data.SetMoveFields(true, DirectionFacing, 1);
            return true;
        }
        else
        {
            data.SetMoveFields(false, Vector2Int.zero, 0);
            return false;
        }
    }

    public void AnimateModule(GridBlock gb)
    {
        throw new System.NotImplementedException();
    }

}
