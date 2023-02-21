using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public int CurrentTick = 1;
    public bool VerboseConsole;
    public bool StateManagerVerboseConsole;
    [SerializeField] bool PlayerUpdateVerboseConsole;

    [TitleGroup("CUSTOM SPAWN SEQUENCES")]
    [SerializeField] bool overrideSpawnSequence;
    [ShowIf("overrideSpawnSequence")] [SerializeField] SpawnWave[] customSpawnSequence;

    GamePhase mainGamePhase;

    //  #FIELDS
    [HideInInspector] public enum GameState { Launch, Player, Board};
    StateManager <GameState> stateManager = new StateManager<GameState>();

    Module.UsageData previousModuleData;
    bool newDirectionButtonPressed = false;
    bool activateModuleButtonPressed = false;
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

        inputM = GetComponent<InputManager>();
        inputM.ActivateModuleButtonPressed += ToggleModuleActivation;
        inputM.MoveButtonPressed += OnPlayerMove;
        inputM.EndTurnButtonPressed += EndPlayerTurn;
        

        PlayerHUD pHUD = FindObjectOfType<PlayerHUD>();
                

        mainGamePhase = GamePhase.Player;
    }
    private void Start()
    {
        stateManager.SwitchState(GameState.Launch);
        stateManager.SwitchState(GameState.Player);
    }
    private void Update()
    {
        stateManager.UpdateState();
    }

    //  #METHODS
    private void LaunchEnter()
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
        player.Init();
        player.NextLevel(level.jumpFuelAmount);
        //player.SubscribeToInputActions(inputM.ChangeDirectionButtonPressed, inputM.NewModuleButtonPressed);
        inputM.ChangeDirectionButtonPressed += player.ChangeDirectionFacing;
        inputM.NewModuleButtonPressed += player.SelectNewModule;


        // Initialize SpawnManager
        spawnM.Init(level);
        if (spawnM.CustomSpawnSequenceExist)
        {

        }
        gridObjectM.GridObjectHasDeparted += spawnM.ReclaimSpawn;

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

    private void PlayerEnter()
    {
        if (StateManagerVerboseConsole)
            Debug.Log("GameManager.PlayerEnter() called.");

        gridObjectM.currentGamePhase = GamePhase.Player;
        playerTurnFulfilled = false;
        inputM.SetInputActive(true);
    }

    private void PlayerUpdate()
    {
        if (StateManagerVerboseConsole)
            Debug.Log("GameManager.PlayerUpdate() called.");

        if (activateModuleButtonPressed)
        {
            if (PlayerUpdateVerboseConsole)
                Debug.Log("GameManager.activateModuleButtonPressed = true");

            activateModuleButtonPressed = false;

            player.UseCurrentModule();
            if (player.thrusterUsageData != null)
            {
                inputM.SetInputActive(false);
                gridObjectM.OnPlayerActivateModule(player.thrusterUsageData);
                playerTurnFulfilled = true;
                gridCurrentlyAnimating = true;
            }
            else if (player.weaponUsageData != null)
            {
                Weapon.UsageData uData = player.weaponUsageData;
                gridObjectM.OnPlayerActivateModule(uData);

                if (uData.DoesPlaceObjectInWorld)
                {
                    gridObjectM.NewGridUpdateSteps(includePlayer: false);
                    gridObjectM.LoadGridUpdateSteps();
                    gridObjectM.RunGridUpdate();
                    gridObjectM.AnimateMovement();
                    gridCurrentlyAnimating = true;
                }
            }
            // This needs to be set up as a delegate
            // pHUD.RefreshHUDEntry(uData.newAmmoAmount, false);
            else
            {
                Debug.Log("No uData available for current module.");
            }
        }

        if (endTurnReceived)
        {
            if (PlayerUpdateVerboseConsole)
                Debug.Log("GameManager.endTurnReceived = true");
            
            endTurnReceived = false;
        }

        if (gridCurrentlyAnimating)
        {
            if (PlayerUpdateVerboseConsole)
                Debug.Log("GameManager.gridCurrentlyAnimating = true");

            if (gridObjectM.IsAnimationComplete)
            {
                gridObjectM.ResolveCollisionsOnGridBlocks();
                gridObjectM.NewGridUpdateSteps(checkMove: false, checkLoot: false);
                gridObjectM.RemoveDeadObjectsAndDropLoot();
                gridCurrentlyAnimating = false;
            }
        }

        if (playerTurnFulfilled && gridObjectM.IsAnimationComplete)
            { stateManager.SwitchState(GameState.Board); }
    }   
    
    private void PlayerExit()
    {
        if (StateManagerVerboseConsole)
            Debug.Log("GameManager.PlayerExit() called.");
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
        if (StateManagerVerboseConsole)
            Debug.Log("GameManager.BoardEnter() called.");

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
        if (StateManagerVerboseConsole)
            Debug.Log("GameManager.BoardUpdate() called.");

        if (gridObjectM.IsAnimationComplete)
            stateManager.SwitchState(GameState.Player);
    }

    void BoardExit()
    {
        if (StateManagerVerboseConsole)
            Debug.Log("GameManager.BoardExit() called.");
        
        gridObjectM.ResolveCollisionsOnGridBlocks();
        gridObjectM.NewGridUpdateSteps(checkMove: false, checkHealth: true, checkLoot: true);
        gridObjectM.RemoveDeadObjectsAndDropLoot(); //Is a missile not being removed because of eligibleForProcessing?
        

        //Spawn stuff goes here
        if (spawnM.CountAvailableBorderSpawns > 0 && (spawnM.ForceSpawnEveryTurn || Random.Range(0, 10) > 4))    // 50%
            gridObjectM.ApplySpawnWave(spawnM.GetSpawnWave);
    }

    void ToggleModuleActivation() { activateModuleButtonPressed = true; }


    
    private void OnPlayerActivateModule()
    {
        //pHUD.OnPlayerHUDModuleActivate -= OnPlayerActivateModule;

        //if (previousModuleData != null)
            //StopCoroutine(OnPlayerActivateModuleCoroutine(previousModuleData));
                     
        //StartCoroutine(OnPlayerActivateModuleCoroutine(data));
       // previousModuleData = data;
    }
    /*private IEnumerator OnPlayerActivateModuleCoroutine(Module.UsageData data)
    {
        gridObjectM.OnPlayerActivateModule(data);
        yield return new WaitForSecondsRealtime(2.0f);
        pHUD.OnPlayerHUDModuleActivate += OnPlayerActivateModule;
        yield return null;
    }
    */
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