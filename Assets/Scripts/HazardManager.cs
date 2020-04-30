using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    #region Inspector Attributes
    [SerializeField]
    GameObject[] hazardPrefabs;
    #endregion

    #region
    private GridManager gm;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GridManager>();

        GameObject asteroid = Instantiate(hazardPrefabs[0]);
        gm.PlaceObject(asteroid, new Vector2Int(6, 6));
        gm.hazards.Add(asteroid);
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
