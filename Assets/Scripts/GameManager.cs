using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public int CurrentTick = 1;
    public bool VerboseConsole = true;

    [TitleGroup("CUSTOM SPAWN SEQUENCES")]
    public SpawnSequence[] overrideSpawnSequence;

    [TitleGroup("LEVEL MANAGEMENT")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private bool overrideLevelData;
    [ShowIf("overrideLevelData")] [SerializeField] private int inspectorGridWidth;
    [ShowIf("overrideLevelData")] [SerializeField] private int inspectorGridHeight;

    public int CurrentLevel
    {
        get { return currentLevel; }
    }
    private int currentLevel = 0;
    private int LevelDataIndex
    {
        get { return currentLevel - 1; }
    }


    // REFERENCES
    GameDisplay display;
    GridManager gm;
    GridObjectManager gom;
    Player player;
    DebugHUD debugHUD;
    

    // METHODS
    private void Start()
    {
        gm = GetComponent<GridManager>();

        // Setup level characteristics for GridManager
        if (currentLevel == 0) currentLevel = 1;
        if (overrideLevelData)
        {
            //LevelData newLevelData = new LevelData(inspectorGridWidth, inspectorGridHeight, 4, 0);
            //LevelData newLevelData = CreateInstance<LevelData>();
            LevelData newLevelData = LevelData.CreateInstance<LevelData>();
            newLevelData.AddRowData(inspectorGridWidth, inspectorGridHeight, 4, 10, 1);
            

            gm.ReceiveLevelData(newLevelData.LevelTable[0]);
        }
        else
        {
            gm.ReceiveLevelData(levelData.LevelTable[LevelDataIndex]);
        }
        gm.Init(); 


        // Setup GridObjectManager
        gom = GetComponent<GridObjectManager>();
        if (overrideSpawnSequence.Length > 0)
        {
            for (int i = 0; i < overrideSpawnSequence.Length; i++)
            {               
                gom.insertSpawnSequences.Add(overrideSpawnSequence[i].Clone());
            }
        }

        if (VerboseConsole) debugHUD = GameObject.FindGameObjectWithTag("Debug HUD").GetComponent<DebugHUD>();
        display = GameObject.FindGameObjectWithTag("Game Display").GetComponent<GameDisplay>();


        // Setup Player
        Vector2Int startLocation = new Vector2Int(0, 0);
        if (overrideSpawnSequence.Length > 0)
        {
            startLocation = overrideSpawnSequence[0].playerSpawnLocation;
        }

        GameObject player = Instantiate(gom.playerPrefab, gm.GridToWorld(startLocation), Quaternion.identity);
        if (VerboseConsole) Debug.Log(player.name + " has been instantiated.");

        if (VerboseConsole) Debug.Log("Adding Player to Grid.");
        gm.AddObjectToGrid(player, startLocation);
        this.player = player.GetComponent<Player>();
        this.player.currentWorldLocation = gm.GridToWorld(startLocation);
        this.player.targetWorldLocation = gm.GridToWorld(startLocation);
        if (VerboseConsole) Debug.Log("Player successfully added to Grid.");

        this.player.NextLevel(levelData.LevelTable[LevelDataIndex].jumpFuelAmount);

        // Initialize Game Object Manager now that player exists
        gom.Init();
        gom.NextLevel(levelData.LevelTable[LevelDataIndex].numberOfPhenomenaToSpawn, levelData.LevelTable[LevelDataIndex].numberOfStationsToSpawn);
        gom.InsertManualSpawnSequence();

        this.player.OnPlayerAdvance += OnTick;
    }
   
    private void OnTick()
    {
        StartCoroutine(OnTickCoroutine());
    }
    private IEnumerator OnTickCoroutine()
    {
        if (player.IsAlive)
        {
            player.InputActive = false;
            player.OnPlayerAdvance -= OnTick;

            //player.OnPlayerAddHazard += OnAddHazard;
            float delay = player.OnTickUpdate();
            gom.OnTickUpdate(GridObjectManager.GamePhase.Player);
            yield return new WaitForSeconds(delay);
            if (CheckWinCondition())
            {
                currentLevel++;
                gom.ClearLevel();
                gm.ReceiveLevelData(levelData.LevelTable[LevelDataIndex]);
                gm.NextLevel();   // Don't adjust index
                gom.NextLevel(levelData.LevelTable[LevelDataIndex].numberOfPhenomenaToSpawn, levelData.LevelTable[LevelDataIndex].numberOfStationsToSpawn);
                gom.ArrivePlayer();
                player.NextLevel(levelData.LevelTable[LevelDataIndex].jumpFuelAmount);
            }
            player.OnPlayerAddHazard -= OnAddHazard;
        }
        else
        {
            EndGame();
        }
            
        if (player.IsAlive)
        {
            float hazardDelay = gom.OnTickUpdate(GridObjectManager.GamePhase.Manager);
            yield return new WaitForSeconds(hazardDelay);
            
            CurrentTick += 1;

            player.InputActive = true;
            player.OnPlayerAdvance += OnTick;
        }
        else
        {
            EndGame();
        }    
        
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
        
    private bool CheckWinCondition()
    {
        if (player.CurrentJumpFuel >= levelData.LevelTable[LevelDataIndex].jumpFuelAmount)
            return true;
        else
            return false;
    }
    private void EndGame()
    {
        display.GameOver();
    }

}