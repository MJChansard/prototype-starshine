using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] string hazardName;
    public float moveSpeed = 1.0f;
    public Vector3 currentWorldLocation;
    public Vector3 targetWorldLocation;
    [SerializeField] private GameObject spawnWarningObject;
    
    private HazardMode currentMode;

    public float Distance
    {
        get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
    }

    public string HazardName
    {
        get { return hazardName; }
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
            MeshRenderer mesh = GetComponent<MeshRenderer>();
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
