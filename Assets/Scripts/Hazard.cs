using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Hazard Properties")]
    [SerializeField] string hazardName;
    public HazardType hazardType;
    //[SerializeField] float moveSpeed = 1.0f;

    public float MoveSpeed
    {
        get {return moveSpeed; }
    }

    // Could put movement data object here
    // Been wanting to replace string hazardName with a HazardType enum
    
    public float moveSpeed = 1.0f;      // Animation move speed
    public Vector3 currentWorldLocation;
    public Vector3 targetWorldLocation;

    [Header("Hazard Spawning Options")]
    [SerializeField] private GameObject spawnWarningObject;
    public GridManager.SpawnRule spawnRules;

    private HazardMode currentMode;

    public float Distance
    {
        get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
    }

    public string HazardName
    {
        get { return hazardName; }
    }

    public enum HazardType
    {
        SmallAsteroid = 1,
        LargeAsteroid = 2,
        PlayerMissile = 11
    }


    public enum HazardMode
    { 
        Spawn = 1,
        Play = 2
    }

    public HazardMode CurrentMode
    {
        get
        {
            return currentMode;
        }
    }


    public bool RequiresSpawnAnimation
    {
        get
        {
            return spawnWarningObject != null;
        }
    }

    public void SetHazardAnimationMode(HazardMode mode)
    {
        if (RequiresSpawnAnimation)
        {
            //MeshRenderer mesh = GetComponent<MeshRenderer>();
            MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
            SpriteRenderer sprite = spawnWarningObject.GetComponent<SpriteRenderer>();
            Animator anim = spawnWarningObject.GetComponent<Animator>();

            if (mode == HazardMode.Spawn)
            {
                currentMode = HazardMode.Spawn;
                mesh.enabled = false;
                sprite.enabled = true;
                anim.SetBool("InSpawnMode", true);
            }

            if (mode == HazardMode.Play)
            {
                currentMode = HazardMode.Play;
                sprite.enabled = false;
                anim.SetBool("InSpawnMode", false);
                mesh.enabled = true;
            }
        }
        else
        {
            currentMode = mode;
        }
    }
}

