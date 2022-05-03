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
    [ShowIf("overrideSpawnSequence")] [SerializeField] SpawnWave[] customSpawnSequence;

    GamePhase mainGamePhase;

    //  #FIELDS
    [HideInInspector] public enum GameState { Launch, Player, Board};
    StateManager <GameState> stateManager = new StateManager<GameState>();

    Module.UsageData previousModuleData;
    bool activateModuleReceived = false;
    bool playerMoveReceived = false;
    bool endTurnReceived = false;
    bool playerTurnFulfilled = false;
    bool gridCurrentlyAnimating = false;

    public System.Action EndOfTurn;
    
    //  #REFERENCES
    GameDisplay display;
    InputManager inputM;
    GridObjectManager gridObjectM;
    GridManager gridM;
    SpawnManager spawnM;
    LevelManager levelM;
    PlayerHUD pHUD;    
    Player player;
    DebugHUD debugHUD;



    
    void Awake()
    {
        gridM = GetComponent<GridManager>();
        gridObjectM = GetComponent<GridObjectManager>();
        spawnM = GetComponent<SpawnManager>();
        levelM = GetComponent<LevelManager>();
        debugHUD = GameObject.FindGameObjectWithTag("Debug HUD").GetComponent<DebugHUD>();
        display = GameObject.FindGameObjectWithTag("Game Display").GetComponent<GameDisplay>();

        stateManager.RegisterStateMethods(this, true);

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
        stateManager.SwitchState(GameState.Launch);
        stateManager.SwitchState(GameState.Player);
    }
    private void Update()
    {
        stateManager.UpdateState();
    }

    //  #METHODS
    void LaunchEnter()
    {
        LevelRecord level = levelM.CurrentLevelData;
        /*
        if (customSpawnSequence.Length > 0)
        {
            for (int i = 0; i < customSpawnSequence.Length; i++)
            {
                gridObjectM.insertSpawnSequences.Add(customSpawnSequence[i].Clone());
            }
        }
        */

        if (spawnM.CustomSpawnSequenceExist)
        {
            
        }

        
        // Setup level characteristics for GridManager        
        gridM.ReceiveLevelData(level);
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
        if (VerboseConsole) Debug.Log("Player successfully added to Grid.");
        player.NextLevel(levelM.CurrentLevelData.jumpFuelAmount);
        


        // Initialize Game Object Manager now that player exists
        gridObjectM.Init();
        gridObjectM.NextLevel(level.numberOfPhenomenaToSpawn, level.numberOfStationsToSpawn);
        gridObjectM.InsertManualSpawnSequence();


        pHUD = GameObject.FindGameObjectWithTag("Player HUD").GetComponent<PlayerHUD>();
        if (pHUD != null)
        {
            Debug.Log("GameManager successfully located PlayerHUD.");
            pHUD.Init(player.GetEquippedModules, level.jumpFuelAmount, player.GetComponent<Health>().MaxHP);
        }
        else
        {
            Debug.Log("GameManager failed to locate PlayerHUD.");
        }
    }

    void PlayerEnter()
    {
        gridObjectM.currentGamePhase = GamePhase.Player;
        playerTurnFulfilled = false;
        inputM.SetInputActive(true);
    }

    void PlayerUpdate()
    {
        // Could potentially put movement stuff in here
        if (activateModuleReceived)
        {
            activateModuleReceived = false;
            
            Module.UsageData uData = player.UseCurrentModule();
            if (uData != null)
            {
                gridObjectM.OnPlayerActivateModule(uData);
                if (uData.doesPlaceObjectInWorld)
                {
                    gridObjectM.NewGridUpdateSteps(includePlayer: false);
                    gridObjectM.LoadGridUpdateSteps();
                    gridObjectM.RunGridUpdate();
                    gridObjectM.AnimateMovement();
                    gridCurrentlyAnimating = true;
                }
                pHUD.RefreshHUDEntry(uData.newAmmoAmount, false);
            }
            else
            {
                Debug.Log("No uData available for current module.");
            }
        }
        

        if (playerMoveReceived)
        {
            playerMoveReceived = false;
            playerTurnFulfilled = true;
            inputM.SetInputActive(false);
            gridObjectM.NewGridUpdateSteps();
            gridObjectM.LoadGridUpdateSteps();
            gridObjectM.RunGridUpdate();
            gridObjectM.AnimateMovement();
            gridCurrentlyAnimating = true;
        }

        if (endTurnReceived)
        {
            endTurnReceived = false;
        }

        if (gridCurrentlyAnimating)
        {
            if (gridObjectM.IsAnimationComplete)
            {
                gridObjectM.ResolveCollisionsOnGridBlocks();
                gridObjectM.NewGridUpdateSteps(checkMove: false, checkLoot: false);
                gridObjectM.RemoveDeadObjectsAndDropLoot();
                gridCurrentlyAnimating = false;
            }
        }

        if (playerTurnFulfilled && gridObjectM.IsAnimationComplete)
            stateManager.SwitchState(GameState.Board);
    }   
    
    void PlayerExit()
    {
        //gridObjectM.AnimateMovement();
        gridObjectM.ResolveCollisionsOnGridBlocks();
        gridObjectM.NewGridUpdateSteps(checkHealth: true, checkMove: false, checkLoot: true);
        gridObjectM.LoadGridUpdateSteps();
        gridObjectM.RemoveDeadObjectsAndDropLoot();
        
        if (EndOfTurn != null)
            EndOfTurn();
    }

    void BoardEnter()
    {
        gridObjectM.currentGamePhase = GamePhase.Board;
        gridObjectM.NewGridUpdateSteps();
        gridObjectM.RemoveDeadObjectsAndDropLoot();
        gridObjectM.LoadGridUpdateSteps();

        gridObjectM.RunGridUpdate();
        gridObjectM.AnimateMovement();
        //stateManager.SwitchState(GameState.Animate);
    }   
    
    void BoardUpdate()
    {
        if (gridObjectM.IsAnimationComplete)
            stateManager.SwitchState(GameState.Player);
    }

    void BoardExit()
    {
        gridObjectM.ResolveCollisionsOnGridBlocks();
        gridObjectM.NewGridUpdateSteps(checkMove: false, checkHealth: true, checkLoot: true);
        gridObjectM.RemoveDeadObjectsAndDropLoot(); //Is a missile not being removed because of eligibleForProcessing?
        gridObjectM.SpawnGridObjects();
    }

    void ActivateModule()
    {
        activateModuleReceived = true;
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
    private void EndPlayerTurn()
    {
        endTurnReceived = true;
    }
   
        
    private bool CheckWinCondition()
    {
        if (player.CurrentJumpFuel >= levelM.CurrentLevelData.jumpFuelAmount)   // #OPTIMIZE
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
    Board = 2
}