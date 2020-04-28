using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    #region Inspector Attributes
    [SerializeField]
    GameObject[] hazardArray;
    #endregion

    #region
    private GridManager gm;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GridManager>();

        Instantiate
        (
            hazardArray[0],
            gm.GridToWorld(new Vector2Int(7, 7)),
            Quaternion.identity
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
/*
    private void MoveHazard(GameObject hazard, )
    {

    }
*/
}
