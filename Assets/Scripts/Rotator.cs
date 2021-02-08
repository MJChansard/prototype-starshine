using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] bool UseDefaultValues = true;
    [SerializeField] Vector3 RotateUpValues = new Vector3();
    [SerializeField] Vector3 RotateDownValues = new Vector3();
    [SerializeField] Vector3 RotateLeftValues = new Vector3();
    [SerializeField] Vector3 RotateRightValues = new Vector3();
    
    private float xRotationSpeed;
    private float yRotationSpeed;
    private float zRotationSpeed;
    //[SerializeField] Hazard.Type hazardType;

    private Dictionary<string, Vector3> DefaultRotations = new Dictionary<string, Vector3>()
    {
        {"Up", new Vector3(50.0f, 0.0f, 0.0f) },
        {"Down", new Vector3(-50.0f, 0.0f, 0.0f) },
        {"Left", new Vector3(0.0f, 50.0f, 50.0f) },
        {"Right", new Vector3(0.0f, 50.0f, -50.0f) }
    };  

    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * yRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.right * xRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.forward * zRotationSpeed * Time.deltaTime);
    }

    private void RotateUp()
    {
        if(UseDefaultValues)
        {
            Vector3 values = DefaultRotations["Up"];
            xRotationSpeed = values.x;
            yRotationSpeed = values.y;
            zRotationSpeed = values.z;
        }
        else
        {

            xRotationSpeed = RotateUpValues.x;
            yRotationSpeed = RotateUpValues.y;
            zRotationSpeed = RotateUpValues.z;
        }
    }

    private void RotateDown()
    {
        if (UseDefaultValues)
        {
            Vector3 values = DefaultRotations["Down"];
            xRotationSpeed = values.x;
            yRotationSpeed = values.y;
            zRotationSpeed = values.z;
        }
        else
        {

            xRotationSpeed = RotateDownValues.x;
            yRotationSpeed = RotateDownValues.y;
            zRotationSpeed = RotateDownValues.z;
        }
    }

    private void RotateLeft()
    {
        if (UseDefaultValues)
        {
            Vector3 values = DefaultRotations["Left"];
            xRotationSpeed = values.x;
            yRotationSpeed = values.y;
            zRotationSpeed = values.z;
        }
        else
        {

            xRotationSpeed = RotateLeftValues.x;
            yRotationSpeed = RotateLeftValues.y;
            zRotationSpeed = RotateLeftValues.z;
        }
    }

    private void RotateRight()
    {
        if (UseDefaultValues)
        {
            Vector3 values = DefaultRotations["Right"];
            xRotationSpeed = values.x;
            yRotationSpeed = values.y;
            zRotationSpeed = values.z;
        }
        else
        {

            xRotationSpeed = RotateRightValues.x;
            yRotationSpeed = RotateRightValues.y;
            zRotationSpeed = RotateRightValues.z;
        }
    }

    public void ApplyRotation(ref string border)
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
