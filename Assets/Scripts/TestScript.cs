using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 currentLocation = new Vector3(5.0f, 4.0f, 0.0f);
        Vector3 targetLocation = new Vector3(5.0f, 5.0f, 0.0f);

        Debug.Log(currentLocation - targetLocation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
