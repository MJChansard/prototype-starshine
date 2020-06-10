using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Properties
    public int CurrentTick = 1;
    public int CurrentLevel = 1;
    #endregion

    #region References
    HazardManager hm;
    PlayerManager pm;
    EnemyManager em;
    GridManager gm;
    #endregion

    [SerializeField]
    GameObject playerPrefab;

    private void Start()
    {
        gm = GetComponent<GridManager>();
        gm.Init();
        
        hm = GetComponent<HazardManager>();
        hm.Init();

        em = GetComponent<EnemyManager>();

        // Prepare Player
        GameObject player = Instantiate(playerPrefab);
        Vector2Int startLocation = new Vector2Int(5, 4);
        gm.PlaceObject(player, startLocation);
        pm = player.GetComponent<PlayerManager>();
        pm.currentWorldLocation = gm.GridToWorld(startLocation);
        pm.targetWorldLocation = gm.GridToWorld(startLocation);
        
        pm.OnPlayerAdvance += OnTick;
    }

    private void OnTick()
    {
        StartCoroutine(OnTickCoroutine());
    }

    private IEnumerator OnTickCoroutine()
    {
        pm.OnPlayerAdvance -= OnTick;

        pm.OnPlayerAddHazard += OnAddHazard;
        float delay = pm.OnTickUpdate();
        yield return new WaitForSeconds(delay);
        pm.OnPlayerAddHazard -= OnAddHazard;

        hm.OnTickUpdate();
        
        CurrentTick += 1;
        pm.OnPlayerAdvance += OnTick;
        yield return null;
    }

    private void OnAddHazard(Hazard hazardToAdd, Vector2Int position)
    {
        Debug.Log("OnAddHazard() called.");
        hm.AddHazard(hazardToAdd, position);
    }
}
