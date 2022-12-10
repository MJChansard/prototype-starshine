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
        levelM.Init();
        LevelRecord level = levelM.CurrentLevelData;
  
        // Setup level characteristics for GridManager        
        gridM.ReceiveLevelData(level);
        gridM.Init();
        
        
        // Initialize GridObjectManager
        if (level.levelTopography == null)
            Debug.Log("The topography for this level is NULL.");
        else
            Debug.Log("The topography for this level has been found.");
        gridObjectM.Init(level);
        //gridObjectM.NextLevel(level.numberOfPhenomenaToSpawn, level.numberOfStationsToSpawn);
        

        // Cache reference and setup Player
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.NextLevel(level.jumpFuelAmount);


        // Initialize SpawnManager
        spawnM.Init();
        if (spawnM.CustomSpawnSequenceExist)
        {

        }

        // Spawn GridObjects
        gridObjectM.ApplySpawnWave(spawnM.GetSpawnWave);

        /*  STORING AS ALTERNATE IMPLEMENTATION
        bool spawnCountBelowThreshold = true;
        int spawnCount = 0;
        while (spawnCountBelowThreshold)
        {
            SpawnWave wave = spawnM.GetSpawnWave;
            spawnCount += wave.spawns.Length;
            gridObjectM.ApplySpawnWave(wave);

            if (spawnCount >= level.MaxObjectsOnGrid)
                spawnCountBelowThreshold = false;
        }      
        */

        // PlayerHUD
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
            
            player.UseCurrentModule();
            if (player.thrusterUsageData != null)
            {
                gridObjectM.OnPlayerActivateModule(player.thrusterUsageData);
            }
            else if (player.weaponUsageData != null)
            {
                gridObjectM.OnPlayerActivateModule(player.weaponUsageData);
            }

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

        //Spawn stuff goes here
        if (spawnM.ForceSpawnEveryTurn || Random.Range(0, 10) > 4)    // 50%
            gridObjectM.ApplySpawnWave(spawnM.GetSpawnWave);
            
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