using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] float xRotationSpeed;
    [SerializeField] float yRotationSpeed;
    [SerializeField] float zRotationSpeed;
    [SerializeField] Hazard.HazardType hazardType;
   
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * yRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.right * xRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.forward * zRotationSpeed * Time.deltaTime);
    }

    public void RotateUp()
    {
        if(hazardType == Hazard.HazardType.Comet)
        {

        }
        else
        {
            xRotationSpeed = 50.0f;
            yRotationSpeed = 0.0f;
            zRotationSpeed = 0.0f;
        }

    }

    public void RotateDown()
    {
        xRotationSpeed = -50.0f;
        yRotationSpeed = 0.0f;
        zRotationSpeed = 0.0f;
    }

    public void RotateLeft()
    {
        xRotationSpeed = 0.0f;
        yRotationSpeed = 50.0f;
        zRotationSpeed = 50.0f;
    }

    public void RotateRight()
    {
        xRotationSpeed = 0.0f;
        yRotationSpeed = 50.0f;
        zRotationSpeed = -50.0f;
    }

    public void ApplyRotation(Hazard.HazardType type, string border)
    {
        if (type == Hazard.HazardType.Comet)
        {
            xRotationSpeed = 0.0f;
            yRotationSpeed = 20.0f;
            zRotationSpeed = 0.0f;
        }
        else
        {
            switch(border)
            {
                case "Bottom":
                    RotateUp();
                    break;

                case "Top":
                    RotateDown();
                    break;

                case "Left":
                    RotateRight();
                    break;

                case "Right":
                    RotateLeft();
                    break;
            }
                

        }
    }
}
