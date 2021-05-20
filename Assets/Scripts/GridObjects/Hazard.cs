using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : GridObject
{
    [Header("Hazard Properties")]
    [SerializeField] private Type hazardType;
    public Type HazardType { get { return hazardType; } }

    public string HazardName { get { return hazardType.ToString("G"); } }

    public enum Type
    {
        SmallAsteroid = 1,
        LargeAsteroid = 2,
        Comet = 3,
        Shipwreck = 4,
        AmmoCrate = 5,
        PlayerMissile = 11
    }

    public bool RequiresSpawnAnimation
    {
        get
        {
            return spawnWarningObject != null;
        }
    }

    public override void Init(string spawnBorder)
    {
        base.Init(spawnBorder);

    }

    public override void SetGamePlayMode(Mode mode)
    {
        if (RequiresSpawnAnimation)
        { 
            MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
            SpriteRenderer sprite = spawnWarningObject.GetComponent<SpriteRenderer>();
            Animator anim = spawnWarningObject.GetComponent<Animator>();

            ParticleSystem particleSystem1 = GetComponent<ParticleSystem>(); ;
            ParticleSystem particleSystem2 = GetComponentInChildren<ParticleSystem>();

            Health health = GetComponent<Health>();

            if (mode == Mode.Spawn)
            {
                currentMode = Mode.Spawn;
                mesh.enabled = false;
                sprite.enabled = true;
                anim.SetBool("InSpawnMode", true);

                if(HazardType == Type.Comet)
                {
                    particleSystem1.Stop();
                    particleSystem2.Stop();
                }
            }

            if (mode == Mode.Play)
            {
                currentMode = Mode.Play;
                sprite.enabled = false;
                anim.SetBool("InSpawnMode", false);
                mesh.enabled = true;
                health.ToggleInvincibility(false);

                if(HazardType == Type.Comet)
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

//moveSpeed = 1.0f;      // Animation move speed
//public Vector3 currentWorldLocation;
//public Vector3 targetWorldLocation;

//[Header("Hazard Spawning Options")]
//public GameObject spawnWarningObject;
//public GridManager.SpawnRule spawnRules;

//private HazardMode currentMode;

/*
public float Distance
{
    get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
}
*/

/*
public enum HazardMode
{ 
    Spawn = 1,
    Play = 2
}
*/
/*
    public HazardMode CurrentMode
    {
        get
        {
            return currentMode;
        }
    }
*/