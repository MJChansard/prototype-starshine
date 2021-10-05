using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GridObject : MonoBehaviour
{
    [Header("General Properties")]
    public GridObjectManager.GamePhase ProcessingPhase;


    [TitleGroup("SPAWN SETTINGS")]
    public bool RequiresSpawnWarning;
    [ShowIf("RequiresSpawnWarning")] public GameObject spawnWarningObject;
    public GridManager.SpawnRule spawnRules;
    

    // MOVEMENT & ANIMATION FIELDS
    [HideInInspector] public Vector3 currentWorldLocation;      
    [HideInInspector] public Vector3 targetWorldLocation;       
    [HideInInspector] public float animateMoveSpeed;
    [HideInInspector] public bool IsLeavingGrid = false;
    public float Distance
    {
        get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
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