using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private GameObject player;
    private PlayerManager pm;
    
    private Vector3 gridLimits;
    [SerializeField] private Vector3 Result = new Vector3();

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            pm = player.GetComponent<PlayerManager>();

            gridLimits = new Vector3(8, 10, 0);

            Result = new Vector3
            (
                pm.ReportDirection.x * gridLimits.x,
                pm.ReportDirection.y * gridLimits.y,
                0.0f
            );
        }

        if (player != null)
        {
            Result = new Vector3
            (
                pm.ReportDirection.x * gridLimits.x,
                pm.ReportDirection.y * gridLimits.y,
                0.0f
            );
        }
        //Debug.LogFormat("Resulting Vector3: {0}", testGridLimit);
    }
}
