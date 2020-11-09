using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Properties
    public int CurrentTick = 1;
    public int CurrentLevel = 1;
    
    public bool EnableDebug = true;
    #endregion

    #region References
    HazardManager hm;
    PlayerManager pm;
    EnemyManager em;
    GridManager gm;
    DebugHUD debugHUD;
    #endregion

    [SerializeField] GameObject playerPrefab;

    private void Start()
    {
        gm = GetComponent<GridManager>();
        gm.Init();
        
        
        hm = GetComponent<HazardManager>();
        hm.Init();
        
        em = GetComponent<EnemyManager>();

        if (EnableDebug) debugHUD = GameObject.FindGameObjectWithTag("Debug HUD").GetComponent<DebugHUD>();

        // Prepare Player
        Vector2Int startLocation = new Vector2Int(5, 4);
        GameObject player = Instantiate(playerPrefab, gm.GridToWorld(startLocation), Quaternion.identity);
        Debug.Log(player.name + " has been instantiated.");

        gm.AddObjectToGrid(player, startLocation);
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
        pm.InputActive = false;
        pm.OnPlayerAdvance -= OnTick;

        pm.OnPlayerAddHazard += OnAddHazard;
        float delay = pm.OnTickUpdate();
        yield return new WaitForSeconds(delay);
        pm.OnPlayerAddHazard -= OnAddHazard;

        float hazardDelay = hm.OnTickUpdate();
        yield return new WaitForSeconds(hazardDelay);
        // pm.CheckHP();
        
        CurrentTick += 1;
        gm.ResetSpawns();
        pm.InputActive = true;
        pm.OnPlayerAdvance += OnTick;

        // Debug Options
        if (EnableDebug)
        {
            debugHUD.IncrementTickValue(CurrentTick);
        }
        yield return null;
    }

    private void OnAddHazard(Hazard hazardToAdd, Vector2Int position, bool placeOnGrid = true)
    {
        Debug.Log("GameManager.OnAddHazard() called.");
        hm.AddHazard(hazardToAdd, position, placeOnGrid);
    }
}
