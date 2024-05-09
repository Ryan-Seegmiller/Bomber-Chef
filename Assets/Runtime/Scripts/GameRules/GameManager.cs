using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


//TODO: for Rob GameManager
public class GameManager : MonoBehaviour
{
    #region Declerations
    static public GameManager instance;

    public CameraSwivel cameraSwivel;
    public GameRules gameRules;
    public float conveyourSpeed = 2.5f;

    public Pathfinding playableArea;//The playable area
    public GameState gameState;
    public LevelGraph grid => playableArea.grid;//The grid of the arae

    #region Lists
    public CombatantsScriptableObject players;//List of all the combatants
    public RecipesScriptableObject recipes;//All the available recipes
    [HideInInspector]public CombatantSpawn[] spawnPoints;//All the aailable spawn points
    [HideInInspector]public CombatantActions[] combatants;//All combatants in the scene
    [HideInInspector]public Oven[] ovens;//All the ovens in the scene
    [HideInInspector]public List<BaseBomb> bombs = new List<BaseBomb>();//All the active bombs in the scene
    #endregion

    public bool playerDead;

    #region Duration
    public int combatantsLeft = 0;
    public float gameDuration = 10f;
    private float timeRemaining;

    private float timeTillStart;
    public float startDelayDuration = 3f;
    private Vector3 startTimerStartSize = Vector3.one;
    public GameStartCountdown startCountdown;
    private int _startTimerIntValue;
    private int startTimerIntValue
    {
        get { return _startTimerIntValue; }
        set
        {
            if(_startTimerIntValue == value) { return; }
            startCountdown.transform.localScale = startTimerStartSize;
            startTimerStartSize = startCountdown.transform.localScale;
            startCountdown.transform.localScale *= 1.5f;
            _startTimerIntValue = value;
        }
    }
    #endregion

    UIManager uiManager => UIManager.instance;

    #region DEBUG

    #if UNITY_EDITOR
    [Header("DEBUG")]    
    public DebugManager debugManager;
    #endif
    #endregion

    #endregion

    #region Setup
    public static GameManager GetInstance()
    {
        //Singleton pattern
        if (instance == null)
        {
            return new GameManager();
        }
        else
        {
            return instance;
        }
    }
    GameManager()
    {
        instance = this;
    }
    private void Awake()
    {
        LoadGameRules();
        //Sets the game duration in minutes
        gameDuration = gameRules.duration;
        gameDuration *= 60f;
        //Sets the game state
        gameState = GameState.PreGame;
        timeTillStart = startDelayDuration;
    }
    private void Start()
    {
        StartCoroutine(DelayedStart());
        cameraSwivel = FindObjectOfType<CameraSwivel>();
        uiManager.SetGameTimeText(gameDuration);
    }
    /// <summary>
    /// Loads the game rules
    /// </summary>
    private void LoadGameRules()
    {
        gameRules.health = (int)PlayerPrefs.GetFloat(GameRules.healthGameRuleKey);
        
        gameRules.cookTime = PlayerPrefs.GetFloat(GameRules.CookTimeKey);
        gameRules.duration = PlayerPrefs.GetFloat(GameRules.durationKey);
        gameRules.respawnTime = PlayerPrefs.GetFloat(GameRules.respawnTimeKey);
    }

    /// <summary>
    /// Spawns all the players
    /// </summary>
    private void SpawnPlayers()
    {
        CombatantSpawn[] alreadySpawned = new CombatantSpawn[spawnPoints.Length];
        
        for (int i = 0; i < players.combatants.Length; i++)
        {
            CombatantSpawn spawn = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            while (alreadySpawned.Contains(spawn))
            {
                spawn = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            }
            alreadySpawned[i] = spawn;
            spawn.SpawnPlayer(players.combatants[i], UIManager.instance.playerHud);
            
        }
    }
  
    /// <summary>
    /// Gets all the ivens in the world
    /// </summary>
    public void FindOvens()
    {
        ovens = FindObjectsOfType<Oven>();
        for (int i = 0; i < ovens.Length; i++)
        {
            ovens[i].index = i;
        }
    }
    public void FindCombatants()
    {
        combatants = FindObjectsOfType<CombatantActions>();
        combatantsLeft = combatants.Length;
    }
    /// <summary>
    /// Gets all te spawn points in the world
    /// </summary>
    public void FindSpawnPoints()
    {
        spawnPoints = FindObjectsOfType<CombatantSpawn>();
    }
    #endregion

    private void Update()
    {
        GameTimer();
        StartGameTimer();
    }

    #region Bombs
   /// <summary>
   /// Adds a bomb to the bombs list
   /// </summary>
   /// <param name="bomb"></param>
    public void AddBomb(BaseBomb bomb)
    {
        bombs.Add(bomb);
    }
    /// <summary>
    /// Removes a bomb form the bombs list
    /// </summary>
    /// <param name="bomb"></param>
    public void RemoveBomb(BaseBomb bomb)
    {
        bombs.Remove(bomb);
    }
    #endregion

    #region Runtime Methods
    /// <summary>
    /// Selectes a random recipe from the recipes list
    /// </summary>
    /// <returns></returns>
    public Recipe GetRandomRecipe()
    {
        return recipes.recipes[UnityEngine.Random.Range(0, recipes.recipes.Length)];
    }
    /// <summary>
    /// Gets a random spawn point
    /// </summary>
    /// <returns></returns>
    public CombatantSpawn GetRandomSpawnPoint()
    {
        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
    }
    #endregion

    #region Timer
    private void GameTimer()
    {
        if(gameState != GameState.Playing) { return; }
        if(timeRemaining == 0 && gameState == GameState.Playing)
        {
            timeRemaining = gameDuration;
        }
        if(timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
            gameState = GameState.End;
            GameOver();
        }
        uiManager.SetGameTimeText(timeRemaining); 
    }
    private void StartGameTimer()
    {
        if (gameState != GameState.PreGame) { return; }
        timeTillStart -= Time.deltaTime;
        if(timeTillStart > 0)
        {
            //Does a animatied countdown
            startTimerIntValue = (int)timeTillStart + 1;
            startCountdown.transform.localScale = Vector3.Lerp(startTimerStartSize * 1.5f, startTimerStartSize, timeTillStart % 1);
            startCountdown.cvGroup.alpha = Mathf.Lerp(0, 1, timeTillStart % 1);
            startCountdown.SetText(startTimerIntValue.ToString(), Color.green);

            return;
        }
        startCountdown.transform.parent.gameObject.SetActive(false);
        gameState = GameState.Playing;
        

    }
    #endregion

    #region DeathCheck
    public void EndCheck()
    {
        combatantsLeft--;
        int combantantsDead = 0;
        for (int i = 0; i < combatants.Length; i++)
        {   //Iterates the comabatants dead for each dead combatant
            if (combatants[i].isDead){ combantantsDead++; }
            //TODO: cahnge this check when added local multiplayer
            if (!combatants[i].isAI && !playerDead && combatants[i].isDead)
            {
                OnPlayerDead();
            }
        }
        if (combantantsDead >= combatants.Length - 1)
        {
            GameOver();
        }
    }

    public void OnPlayerDead() 
    { 
        playerDead = true;
        uiManager.PlayerDead();
    }
    public void GameOver()
    {
        gameState = GameState.End;
        uiManager.GameOver(combatants);
    }
    #endregion

    #region Button method
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Happens after start
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        FindOvens();
        FindSpawnPoints();
        SpawnPlayers();
        FindCombatants();
    
        #if UNITY_EDITOR
        if(debugManager != null)
        {
            debugManager.OnSetup();
        }
        #endif
    }
   
    #endregion

}

public enum GameState
{
    PreGame,
    Playing,
    End,
    Pause
}
