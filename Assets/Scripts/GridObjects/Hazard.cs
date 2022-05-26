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

    [HideInInspector] public string spawnBorder;
    private GameObject meshObject;

    public void Init(GridBorder border)
    {
        base.Init();

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
            {
                meshObject = renderer.gameObject;
                break;
            }
        }

        Health hp = GetComponent<Health>();
        if (hp != null) hp.ToggleInvincibility(true);

        GridMover movement = GetComponent<GridMover>();
        if (movement != null)
        {
            switch (border)
            {
                case GridBorder.Bottom:
                    movement.SetMovePatternUp();
                    break;

                case GridBorder.Top:
                    if (spawnRules.requiresOrientation)
                        meshObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);

                    movement.SetMovePatternDown();
                    break;

                case GridBorder.Right:
                    if (spawnRules.requiresOrientation)
                        meshObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                        
                    movement.SetMovePatternLeft();
                    break;

                case GridBorder.Left:
                    if (spawnRules.requiresOrientation)
                        meshObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
                    movement.SetMovePatternRight();
                    break;
            }

            Rotator r = GetComponent<Rotator>();
            if (r != null)
            {
                if (!r.enabled) r.enabled = true;
                r.ApplyRotation(border);
            }
        }
    }

    public override void SetGamePlayMode(Mode mode)
    {
        //if (RequiresSpawnAnimation)
        //if (RequiresSpawnObject)
        if (RequiresSpawnWarning)
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