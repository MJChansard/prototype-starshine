using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Properties
    public int CurrentTick = 1;
    public int CurrentLevel = 1;
    
    public bool EnableDebug = true;

    [Header("Spawn Sequences")]
    public SpawnSequence[] overrideSpawnSequence;   // Don't make this an array
    #endregion

    #region References
    GridObjectManager gom;
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
                
        gom = GetComponent<GridObjectManager>();
        
        // Spawn Overrides
        if (overrideSpawnSequence != null)
        {


            for (int i = 0; i < overrideSpawnSequence.Length; i++)
            {
                //SpawnSequence insert = new SpawnSequence();
                // GM should be responsible for passing clean, reliable data to HM
                // HM should be free to do what it needs to 
                
                gom.insertSpawnSequences.Add(overrideSpawnSequence[i].Clone());
            }
        }

        if (EnableDebug) debugHUD = GameObject.FindGameObjectWithTag("Debug HUD").GetComponent<DebugHUD>();     

        // Prepare Player
        Vector2Int startLocation = new Vector2Int(0, 0);
        if (overrideSpawnSequence.Length > 0)
        {
            startLocation = overrideSpawnSequence[0].playerSpawnLocation;
        }
            
        GameObject player = Instantiate(playerPrefab, gm.GridToWorld(startLocation), Quaternion.identity);
        Debug.Log(player.name + " has been instantiated.");

        gm.AddObjectToGrid(player, startLocation);
        pm = player.GetComponent<PlayerManager>();
        pm.currentWorldLocation = gm.GridToWorld(startLocation);
        pm.targetWorldLocation = gm.GridToWorld(startLocation);

        // Initialze Game Object Manager now that player exists
        gom.Init();

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

        float hazardDelay = gom.OnTickUpdate();
        yield return new WaitForSeconds(hazardDelay);
        // pm.CheckHP();
        
        CurrentTick += 1;
        //gm.ResetSpawns();
        //pm.GatherLoot(gm.WorldToGrid(pm.currentWorldLocation));
        pm.InputActive = true;
        pm.OnPlayerAdvance += OnTick;

        // Debug Options
        if (EnableDebug)
        {
            debugHUD.IncrementTickValue(CurrentTick);
        }
        yield return null;
    }

    private void OnAddHazard(GridObject gridObjectToAdd, Vector2Int position, bool placeOnGrid = true)
    {
        Debug.Log("GameManager.OnAddHazard() called.");
        gom.AddGridObject(gridObjectToAdd, position, placeOnGrid);
    }

}
