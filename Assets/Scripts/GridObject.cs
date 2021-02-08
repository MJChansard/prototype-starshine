using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridObject : MonoBehaviour
{
    [Header("Spawn Properties")]
    public GameObject spawnWarningObject;
    public GridManager.SpawnRule spawnRules;
    
    // Animation & Movement Settings
    public float moveSpeed;
    public Vector3 currentWorldLocation;
    public Vector3 targetWorldLocation;
    
    public float Distance
    {
        get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
    }

    // Object Modes
    protected Mode currentMode;

    public enum Mode
    {
        Spawn = 1,
        Play = 2
    }

    // METHODS
    public virtual void SetAnimationMode(Mode newMode)
    {
 
        MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
        SpriteRenderer sprite = spawnWarningObject.GetComponent<SpriteRenderer>();
        Animator anim = spawnWarningObject.GetComponent<Animator>();

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
        }
    }

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
                        spawnWarningObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                    }
                    movement.SetMovePatternRight();
                    break;
            }
        }

        Rotator r = GetComponent<Rotator>();
        if (r != null) r.ApplyRotation(ref spawnBorder);
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