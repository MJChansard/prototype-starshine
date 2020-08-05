using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] float xRotationSpeed = 50.0f;
    [SerializeField] float yRotationSpeed = 50.0f;
    [SerializeField] float zRotationSpeed = 50.0f;
    Transform objectToRotate;

    private void Start()
    {
         objectToRotate = gameObject.transform;
    }
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * yRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.right * xRotationSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.forward * zRotationSpeed * Time.deltaTime);
    }
}
