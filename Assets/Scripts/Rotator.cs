using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] float xRotationSpeed = 50.0f;
    [SerializeField] float yRotationSpeed = 50.0f;
    [SerializeField] float zRotationSpeed = 50.0f;
   
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * yRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.right * xRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.forward * zRotationSpeed * Time.deltaTime);
    }

    public void RotateUp()
    {
        xRotationSpeed = 50.0f;
        yRotationSpeed = 0.0f;
        zRotationSpeed = 0.0f;
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
}
