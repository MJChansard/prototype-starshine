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
    public GameObject spawnWarningObject;
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
        Comet =  3,
        Shipwreck = 4,
        AmmoCrate = 5,
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

    public void SetHazardAnimationMode(HazardMode mode, HazardType type)
    {
        if (RequiresSpawnAnimation)
        { 
            MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
            SpriteRenderer sprite = spawnWarningObject.GetComponent<SpriteRenderer>();
            Animator anim = spawnWarningObject.GetComponent<Animator>();

            ParticleSystem particleSystem1 = GetComponent<ParticleSystem>(); ;
            ParticleSystem particleSystem2 = GetComponentInChildren<ParticleSystem>();

            if (mode == HazardMode.Spawn)
            {
                currentMode = HazardMode.Spawn;
                mesh.enabled = false;
                sprite.enabled = true;
                anim.SetBool("InSpawnMode", true);

                if(type == HazardType.Comet)
                {
                    particleSystem1.Stop();
                    particleSystem2.Stop();
                }
            }

            if (mode == HazardMode.Play)
            {
                currentMode = HazardMode.Play;
                sprite.enabled = false;
                anim.SetBool("InSpawnMode", false);
                mesh.enabled = true;

                if(type == HazardType.Comet)
                {
                    particleSystem1.Play();
                    particleSystem2.Play();
                }
            }
        }
        else
        {
            currentMode = mode;
        }
    }
}

