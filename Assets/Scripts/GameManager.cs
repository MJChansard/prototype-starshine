using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public int CurrentTick = 1;
    public bool VerboseConsole = true;

    [TitleGroup("CUSTOM SPAWN SEQUENCES")]
    [SerializeField] bool overrideSpawnSequence;
    [ShowIf("overrideSpawnSequence")] [SerializeField] SpawnSequence[] customSpawnSequence;

    [TitleGroup("LEVEL MANAGEMENT")]
    [SerializeField] LevelData levelData;
    [SerializeField] bool overrideLevelData;
    [ShowIf("overrideLevelData")] [SerializeField] int inspectorGridWidth;
    [ShowIf("overrideLevelData")] [SerializeField] int inspectorGridHeight;

    public int CurrentLevel
    {
        get { return currentLevel; }
    }
    int currentLevel = 0;
    int LevelDataIndex
    {
        get { return currentLevel - 1; }
    }
    GamePhase mainGamePhase;

    //  #FIELDS
    [HideInInspector] public enum GameState { Player, Board, Animate};
    StateManager <GameState> stateManager = new StateManager<GameState>();

    Module.UsageData previousModuleData;
    bool activateModuleReceived = false;
    bool playerMoveReceived = false;
    bool endTurnReceived = false;


    //  #REFERENCES
    GameDisplay display;
    InputManager inputM;
    GridObjectManager gridObjectM;
    GridManager gridM;
    PlayerHUD pHUD;    
    Player player;
    DebugHUD debugHUD;



    
    void Awake()
    {
        gridM = GetComponent<GridManager>();
        gridObjectM = GetComponent<GridObjectManager>();
        debugHUD = GameObject.FindGameObjectWithTag("Debug HUD").GetComponent<DebugHUD>();
        display = GameObject.FindGameObjectWithTag("Game Display").GetComponent<GameDisplay>();

        stateManager.RegisterStateMethods(this, true);

        if (currentLevel == 0) currentLevel = 1;

        inputM = FindObjectOfType<InputManager>();
        inputM.ActivateModuleButtonPressed += ActivateModule;
        inputM.MoveButtonPressed += OnPlayerMove;
        inputM.EndTurnButtonPressed += EndPlayerTurn;
        

        PlayerHUD pHUD = FindObjectOfType<PlayerHUD>();
        pHUD.OnPlayerHUDModuleActivate += OnPlayerActivateModule;
        

        mainGamePhase = GamePhase.Player;
    }
    void Start()
    {
        if (customSpawnSequence.Length > 0)
        {
            for (int i = 0; i < customSpawnSequence.Length; i++)
            {
                gridObjectM.insertSpawnSequences.Add(customSpawnSequence[i].Clone());
            }
        }

        // Setup level characteristics for GridManager
        if (overrideLevelData)
        {
            LevelData newLevelData = LevelData.CreateInstance<LevelData>();
            newLevelData.AddRowData(inspectorGridWidth, inspectorGridHeight, 4, 10, 1);

            gridM.ReceiveLevelData(newLevelData.LevelTable[0]);
        }
        else
        {
            gridM.ReceiveLevelData(levelData.LevelTable[LevelDataIndex]);
        }
        gridM.Init();

        // Setup Player
        Vector2Int startLocation = new Vector2Int(0, 0);
        if (customSpawnSequence.Length > 0)
        {
            startLocation = customSpawnSequence[0].playerSpawnLocation;
        }

        GameObject playerObject = Instantiate(gridObjectM.playerPrefab, gridM.GridToWorld(startLocation), Quaternion.identity);
        if (VerboseConsole)
        {
            Debug.Log(playerObject.name + " has been instantiated.");
            Debug.Log("Adding Player to Grid.");
        }
        gridM.AddObjectToGrid(playerObject, startLocation);
        player = playerObject.GetComponent<Player>();
        player.currentWorldLocation = gridM.GridToWorld(startLocation);
        player.targetWorldLocation = gridM.GridToWorld(startLocation);

        if (VerboseConsole) Debug.Log("Player successfully added to Grid.");

        player.NextLevel(levelData.LevelTable[LevelDataIndex].jumpFuelAmount);

        // Initialize Game Object Manager now that player exists
        gridObjectM.Init();
        gridObjectM.NextLevel(levelData.LevelTable[LevelDataIndex].numberOfPhenomenaToSpawn, levelData.LevelTable[LevelDataIndex].numberOfStationsToSpawn);
        gridObjectM.InsertManualSpawnSequence();

        
        pHUD = GameObject.FindGameObjectWithTag("Player HUD").GetComponent<PlayerHUD>();
        if (pHUD != null)
        {
            Debug.Log("GameManager successfully located PlayerHUD.");
            pHUD.Init(player.GetEquippedModules, levelData.LevelTable[LevelDataIndex].jumpFuelAmount, player.GetComponent<Health>().MaxHP);
        }
        else
        {
            Debug.Log("GameManager failed to locate PlayerHUD.");
        }
    }
    private void Update()
    {
        stateManager.UpdateState();
    }

    //  #METHODS

    void PlayerEnter()
    {

    }

    void PlayerUpdate()
    {
        if (activateModuleReceived)
        {
            Module.UsageData uData = player.UseCurrentModule();
            if (uData != null)
            {
                gridObjectM.OnPlayerActivateModule(uData);
                pHUD.RefreshHUDEntry(uData.newAmmoAmount, false);
            }
            activateModuleReceived = false;
        }
        

        if (playerMoveReceived)
        {
            inputM.InputActive = false;
            gridObjectM.OnPlayerMove();
            stateManager.SwitchState(GameState.Animate);
            playerMoveReceived = false;
        }
        
    }   
    
    void PlayerExit()
    {

    }

    void BoardEnter()
    {

    }   
    
    void BoardUpdate()
    {

    }

    void BoardExit()
    {

    }

    void AnimateEnter()
    {

    }

    void ActivateModule()
    {
        activateModuleReceived = true;
    }



    // old
    private void OnTick()
    {
        /*  NOTE
         *  So I think that this method will need to be called when the Player ends the turn.
         *  Based on what the mainGamePhase value is
         */

        /*
        if (mainGamePhase == GamePhase.Player)
        {
            StopCoroutine(OnTickCoroutine(GamePhase.Manager));
            StartCoroutine(OnTickCoroutine(GamePhase.Player));
        }
        else if (mainGamePhase == GamePhase.Manager)
        {
            //StopCoroutine(OnTickCoroutine(GamePhase.Player));
            StartCoroutine(OnTickCoroutine(GamePhase.Manager));
        }
        */
        StartCoroutine(OnTickCoroutine(mainGamePhase));
    }
    private IEnumerator OnTickCoroutine(GamePhase phase)
    {
        /*  NOTES
         * 
         *   - Need to give GameManager the ability to control GamePhase
         *   - GamePhase needs to be long enough for Player to activate all modules
         *   - Each module activation will require a call to GOM.OnTickUpdate
         * 
         */

        if (phase == GamePhase.Player)
        {
            if (player.IsAlive)
            {
                inputM.InputActive = true;
                // Pause the game here so modules can be activated
                yield return null;

                /*
                //gom.OnTickUpdate(GamePhase.Player);

                float delay = player.OnTickUpdate();
                
                
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
                //player.OnPlayerAddHazard -= OnAddHazard;
            }
            else
            {
                EndGame();
            }

            */
            }
        }

        if (phase == GamePhase.Manager)
        {
            if (player.IsAlive)
            {
                float hazardDelay = gridObjectM.OnManagerTickUpdate();
                yield return new WaitForSeconds(hazardDelay);

                CurrentTick += 1;

                //player.InputActive = true;
                UpdateGamePhase();
                OnTick();
                inputM.EndTurnButtonPressed += OnTick;
                inputM.InputActive = true;
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
            //yield return null;
        }
    }
    
    private void OnPlayerActivateModule(Module.UsageData data)
    {
        pHUD.OnPlayerHUDModuleActivate -= OnPlayerActivateModule;

        if (previousModuleData != null)
            StopCoroutine(OnPlayerActivateModuleCoroutine(previousModuleData));
                     
        StartCoroutine(OnPlayerActivateModuleCoroutine(data));
        previousModuleData = data;
    }
    private IEnumerator OnPlayerActivateModuleCoroutine(Module.UsageData data)
    {
        gridObjectM.OnPlayerActivateModule(data);
        yield return new WaitForSecondsRealtime(2.0f);
        pHUD.OnPlayerHUDModuleActivate += OnPlayerActivateModule;
        yield return null;
    }
    private void OnPlayerMove()
    {
        playerMoveReceived = true;
    }



    private void UpdateGamePhase()
    {
        if (mainGamePhase == GamePhase.Player)
            mainGamePhase = GamePhase.Manager;
        else if (mainGamePhase == GamePhase.Manager)
            mainGamePhase = GamePhase.Player;
    }
    private void EndPlayerTurn()
    {
        StopCoroutine(OnTickCoroutine(GamePhase.Player));
        mainGamePhase = GamePhase.Manager;
        OnTick();
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

public enum GamePhase
{
    Player = 1,
    Manager = 2
}