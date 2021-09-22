using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class GridObject : MonoBehaviour
{
    [Header("General Properties")]
    public GridObjectManager.GamePhase ProcessingPhase;


    [Header("Spawn Properties")]
    public GameObject spawnWarningObject;
    public GridManager.SpawnRule spawnRules;
    public bool RequiresSpawnObject { get { return spawnWarningObject != null; } }


    // ANIMATION FIELDS
    [HideInInspector] public Vector3 currentWorldLocation;      
    [HideInInspector] public Vector3 targetWorldLocation;       
    [HideInInspector] public float animateMoveSpeed;
    public float Distance
    {
        get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
    }


    // TICK UPDATE FIELDS
    [HideInInspector] public bool IsLeavingGrid = false;
    
    
    // OBJECT MODES
    public enum Mode
    {
        Spawn = 1,
        Play = 2
    }
    
    public Mode CurrentMode { get { return currentMode; } }
    protected Mode currentMode;

    // METHODS
    public virtual void Init(string spawnBorder)
    {
        currentMode = GridObject.Mode.Spawn;
        Health hp = GetComponent<Health>();
        if (hp != null) hp.ToggleInvincibility(true);

        MovePattern movement = GetComponent<MovePattern>();
        if (movement != null)
        {
            switch (spawnBorder)
            {
                case "Bottom":
                    movement.SetMovePatternUp();
                    break;

                case "Top":
                    if (spawnRules.requiresOrientation)
                    {
                        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                        spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                    }
                    movement.SetMovePatternDown();
                    break;

                case "Right":
                    if (spawnRules.requiresOrientation)
                    {
                        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                        spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                    }
                    movement.SetMovePatternLeft();
                    break;

                case "Left":
                    if (spawnRules.requiresOrientation)
                    {
                        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                        spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                    }
                    movement.SetMovePatternRight();
                    break;
            }
        }

        Rotator r = GetComponent<Rotator>();
        if (r != null)
        {
            if (!r.enabled) r.enabled = true;   
            r.ApplyRotation(spawnBorder);
        }

        //ticksRemainingUntilMove = ticksPerMove;
        if(spawnWarningObject != null) SetGamePlayMode(Mode.Spawn);
    }

    public virtual void SetGamePlayMode(Mode newMode)
    {
        MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
        //SpriteRenderer sprite = spawnWarningObject.GetComponent<SpriteRenderer>();
        //Animator anim = spawnWarningObject.GetComponent<Animator>();
        //Health health = GetComponent<Health>();
               
        if (newMode == Mode.Spawn)
        {
            currentMode = Mode.Spawn;
            mesh.enabled = false;
            
            if (RequiresSpawnObject)
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

            if(RequiresSpawnObject)
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