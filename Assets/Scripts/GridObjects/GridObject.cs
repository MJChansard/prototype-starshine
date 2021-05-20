using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class GridObject : MonoBehaviour
{
    [Header("General Properties")]
    public GridObjectManager.GamePhase ProcessingPhase;
    //[SerializeField] private int ticksPerMove;      // Number of ticks required before a move is requested
    //private int ticksRemainingUntilMove;

    [Header("Spawn Properties")]
    public GameObject spawnWarningObject;
    public GridManager.SpawnRule spawnRules;


    // Animation fields
    [HideInInspector] public Vector3 currentWorldLocation;      // convert this to Vector2Int
    [HideInInspector] public Vector3 targetWorldLocation;       // convert this to Vector2Int
    [HideInInspector] public float moveSpeed;                   // Animation movement speed?
    public float Distance
    {
        get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
    }

    // Tick update fields
    [HideInInspector] public bool IsLeavingGrid = false;
    
    

    // Object Modes
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
        SetGamePlayMode(Mode.Spawn);
    }


    public virtual void SetGamePlayMode(Mode newMode)
    {
        MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
        SpriteRenderer sprite = spawnWarningObject.GetComponent<SpriteRenderer>();
        Animator anim = spawnWarningObject.GetComponent<Animator>();
        Health health = GetComponent<Health>();

        if (newMode == Mode.Spawn)
        {
            currentMode = Mode.Spawn;
            mesh.enabled = false;
            sprite.enabled = true;
            anim.SetBool("InSpawnMode", true);
        }
        else if (newMode == Mode.Play)
        {
            currentMode = Mode.Play;
            sprite.enabled = false;
            anim.SetBool("InSpawnMode", false);
            mesh.enabled = true;

            if (health != null) health.ToggleInvincibility(false);
        }
    }
    // Can use a Type enum here that is a master list
}

/*
public enum Type
{
    Hazard = 1,
    Phenomenon = 2,
    Enemy = 3,
    SupplyCrate = 4
}

//private Type type; 
*/