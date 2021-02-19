using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Properties
    public int CurrentTick = 1;
    public int CurrentLevel = 1;
    
    public bool VerboseConsole = true;

    [Header("Spawn Sequences")]
    public SpawnSequence[] overrideSpawnSequence;   // Don't make this an array
    #endregion

    #region References
    GridManager gm;
    GridObjectManager gom;
    Player pm;

    DebugHUD debugHUD;
    #endregion

    //[SerializeField] GameObject playerPrefab;

    private void Start()
    {
        gm = GetComponent<GridManager>();
        gm.Init();
                
        gom = GetComponent<GridObjectManager>();
        
        // Spawn Overrides
        if (overrideSpawnSequence.Length > 0)
        {
            for (int i = 0; i < overrideSpawnSequence.Length; i++)
            {
                //SpawnSequence insert = new SpawnSequence();
                // GM should be responsible for passing clean, reliable data to HM
                // HM should be free to do what it needs to 
                
                gom.insertSpawnSequences.Add(overrideSpawnSequence[i].Clone());
            }
        }

        if (VerboseConsole) debugHUD = GameObject.FindGameObjectWithTag("Debug HUD").GetComponent<DebugHUD>();     

        // Prepare Player
        Vector2Int startLocation = new Vector2Int(0, 0);
        if (overrideSpawnSequence.Length > 0)
        {
            startLocation = overrideSpawnSequence[0].playerSpawnLocation;
        }

        //GameObject player = Instantiate(playerPrefab, gm.GridToWorld(startLocation), Quaternion.identity);
        GameObject player = Instantiate(gom.playerPrefab, gm.GridToWorld(startLocation), Quaternion.identity);
        if (VerboseConsole) Debug.Log(player.name + " has been instantiated.");

        gm.AddObjectToGrid(player, startLocation);
        pm = player.GetComponent<Player>();
        pm.currentWorldLocation = gm.GridToWorld(startLocation);
        pm.targetWorldLocation = gm.GridToWorld(startLocation);

        // Initialize Game Object Manager now that player exists
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
        gom.OnTickUpdate(GridObjectManager.GamePhase.Player);
        yield return new WaitForSeconds(delay);
        pm.OnPlayerAddHazard -= OnAddHazard;

        float hazardDelay = gom.OnTickUpdate(GridObjectManager.GamePhase.Manager);
        yield return new WaitForSeconds(hazardDelay);
        // pm.CheckHP();
        
        CurrentTick += 1;
        //gm.ResetSpawns();
        //pm.GatherLoot(gm.WorldToGrid(pm.currentWorldLocation));
        pm.InputActive = true;
        pm.OnPlayerAdvance += OnTick;

        // Debug Options
        if (VerboseConsole)
        {
            debugHUD.IncrementTickValue(CurrentTick);
        }
        yield return null;
    }

    private void OnAddHazard(GridObject gridObjectToAdd, Vector2Int position, bool placeOnGrid = true)
    {
        Debug.Log("GameManager.OnAddHazard() called.");
        gom.AddObjectToGrid(gridObjectToAdd, position, placeOnGrid);
    }

}
