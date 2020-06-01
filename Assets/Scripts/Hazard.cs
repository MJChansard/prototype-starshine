using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public Vector3 currentWorldLocation;
    public Vector3 targetWorldLocation;
    
    public  float Distance
    {
        get { return Vector3.Distance(currentWorldLocation, targetWorldLocation); }
    }

    private float moveSpeed = 1.0f;
}
