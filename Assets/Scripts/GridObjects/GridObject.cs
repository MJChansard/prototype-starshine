using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GridObject : MonoBehaviour
{
    //  # INSPECTOR
    [BoxGroup("GRIDOBJECT PROPERTIES", centerLabel: true)]
    //public GameManager.GameState stateForProcessingGridObject;
    public GamePhase processingPhase;

    [TitleGroup("GRIDOBJECT PROPERTIES/SPAWN SETTINGS")]
    [TitleGroup("GRIDOBJECT PROPERTIES/SPAWN SETTINGS")] public bool RequiresSpawnWarning;
    [ShowIf("RequiresSpawnWarning")] public GameObject spawnWarningObject;
    [TitleGroup("GRIDOBJECT PROPERTIES/SPAWN SETTINGS")] public GridManager.SpawnRule spawnRules;


    // MOVEMENT & ANIMATION FIELDS
    Vector2Int gridCurrentLocation;
    Vector2Int gridDestinationLocation;

    public Vector3 animateStartWorldLocation
    {
        get { return new Vector3(gridCurrentLocation.x, gridCurrentLocation.y, 0); }
    }
    public Vector3 animateEndWorldLocation
    {
        get { return new Vector3(gridDestinationLocation.x, gridDestinationLocation.y, 0);  }
    }

    [HideInInspector] public float animateMoveSpeed;
    [HideInInspector] public bool IsLeavingGrid;            //#Deprecated, remove ASAP

    
    public float Distance
    {
        get { return Vector3.Distance(animateStartWorldLocation, animateEndWorldLocation); }
    }
    
    
    // OBJECT MODES
    public enum Mode
    {
        Spawn = 1,
        Play = 2
    }

    protected Mode currentMode;
    public Mode CurrentMode { get { return currentMode; } }
    

    // METHODS
    public virtual void Init()
    {
        IsLeavingGrid = false;
        if (spawnWarningObject != null) SetGamePlayMode(Mode.Spawn);
        else SetGamePlayMode(Mode.Play);
    }

    public virtual void SetGamePlayMode(Mode newMode)
    {
        MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
               
        if (newMode == Mode.Spawn)
        {
            currentMode = Mode.Spawn;
            mesh.enabled = false;
            
            if (RequiresSpawnWarning)
            {
                if (spawnWarningObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
                    sprite.enabled = true;
                if (spawnWarningObject.TryGetComponent<Animator>(out Animator anim))
                    anim.SetBool("InSpawnMode", true);
            }
        }
        else if (newMode == Mode.Play)
        {
            currentMode = Mode.Play;

            if (RequiresSpawnWarning)
            {
                if (spawnWarningObject.TryGetComponent<Animator>(out Animator anim))
                    anim.SetBool("InSpawnMode", false);
                if (spawnWarningObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
                    sprite.enabled = false;
            }

            mesh.enabled = true;

            if (TryGetComponent<Health>(out Health hp))
                hp.ToggleInvincibility(false);
        }
    }
}