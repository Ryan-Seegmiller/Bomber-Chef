using Collectible;
using System.Collections;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class Oven : MonoBehaviour
{
    #region Decleartions
    [HideInInspector] public int index;

    public InventoryManager inventoryManager;
    public Transform bombSpawn;
    public LayerMask itemLayer;

    [HideInInspector]public bool inUse;
    private CombatantActions _combatantInUse;
    public CombatantActions targetdBy = null;

    #region Bombs
    [HideInInspector]public bool ovenLocked = false;
    BaseBomb bombAttached = null;
    #endregion

    #region Timer
    private float timeTillErase = 10f;
    private float timeRemainingTillErase;
    public float cookTimer = 2f;
    private float timeRemainingCook;

    #endregion

    #region Properties
    [HideInInspector]public CombatantActions combatantInUse
    {
        get { return _combatantInUse; }
        set
        {
            if (value)
            {
                targetdBy = null;
            }
            _combatantInUse = value;
            GoapBlackboard.LogState(new WorldState($"Oven: {index} Owned", value));
        }
    }

    UIManager uIManager => UIManager.instance;


    [HideInInspector]public bool isFull => OvenFull();
    [HideInInspector]public bool uiActive => hud.gameObject.activeSelf;
    #endregion

    #region OvenUI
    public OvenHUD hud;
    private RectTransform hudTR;
    public Vector3 hudOffset;

    #endregion

    #endregion

    #region Setup
    private void Start()
    {
        SetupUI();

        inventoryManager = new InventoryManager(hud);
        combatantInUse = null;
        cookTimer = GameManager.instance.gameRules.cookTime;
    }
    private void SetupUI()
    {
        hud = Instantiate(hud, uIManager.transform);
        hudTR = hud.GetComponent<RectTransform>();
        SetUiPosition();
        hud.gameObject.SetActive(false);
    }
    #endregion

    #region Monobehviour
    private void Update()
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }
        SetUiPosition();

        EraseItemsInOven();
        WaitForCook();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(1 << other.gameObject.layer == itemLayer)
        {
            ItemHelper item = other.GetComponent<ItemHelper>();
            AddItem(item);
            item.onPickUp.Invoke();
        }
    }
    #endregion

    #region AddItem
    public void AddItem(ItemHelper item )
    {
        inventoryManager.addIngredient(item);
        
        if (isFull)
        {
            CookBomb();
        }
        else
        {
            StartEraseTimer();
        }
    }
    #endregion

    #region Reset
    public void ResetOven()
    {
        if (combatantInUse)
        { 
            if (combatantInUse.isAI)
            {
                GoapBlackboard.LogState(new WorldState($"BombDone: {combatantInUse.playerName}", true));
                combatantInUse.agent.stateMachine.SetState(AIStates.Attack);
            }
            combatantInUse = null;
        }
        inventoryManager.itemNames.Clear();
        hud.RemoveSprites();
    }
    #endregion

    #region Cook
    private void CookBomb()
    {
        timeRemainingCook = cookTimer;
        inUse = true;
    }
    private void WaitForCook()
    {
        if (!inUse) { return; }
        if(timeRemainingCook > 0)
        {
            timeRemainingCook -= Time.deltaTime;
            hud.fillImage.fillAmount = 1 - ((cookTimer - (timeRemainingCook)) / cookTimer);

        }
        else
        {
            CreateBomb();
        }
    }
    /// <summary>
    /// Creates a bomb at location
    /// </summary>
    void CreateBomb()
    {
        inUse = false;

        string bombMadeName = GameManager.instance.recipes.CheckRecipe(inventoryManager.itemNames);
        ObjectPoolManager.instance.GetGameobjectFromPool(bombMadeName, (GameObject obj) => {
            obj.transform.position = bombSpawn.position;
            BaseBomb bomb = obj.GetComponent<BaseBomb>();
            bomb.bombName = bombMadeName;
            bomb.GetComponent<TrailRenderer>().Clear();
            LockOven(bomb);
            //Pick up callback
            bomb.onPickUp += () =>
            {
                RemoveBomb(bomb);
                bomb.onPickUp = null;
            };
            //Exlosion Callback
            bomb.explosionCallback = () =>
            {
                GameManager.instance.RemoveBomb(bomb);
                ObjectPoolManager.instance.ReturnGameObject(bombMadeName, obj);
            };
            GameManager.instance.AddBomb(bomb);//Adds a bomb to the gamemaneger blackboard
        });
        ResetOven();
    }
    #endregion

    #region Locked Oven
    bool OvenFull()
    {
        return inventoryManager.itemNames.Count == inventoryManager.invertoryCapacity;
    }

    void RemoveBomb(BaseBomb bomb)
    {
        if(ovenLocked && bombAttached == bomb)
        {
            ovenLocked = false;
            bombAttached = null;
        }
    }
    void LockOven(BaseBomb bomb)
    {
        ovenLocked = true;
        bombAttached = bomb;
        SetUIInActive();
    }
    #endregion

    #region Oven CleanUp
    void StartEraseTimer()
    {
        timeRemainingTillErase = timeTillErase;
    }
    void EraseItemsInOven()
    {
        if(inventoryManager.itemNames.Count <= 0) { return; }
        if(timeRemainingTillErase > 0)
        {
            timeRemainingTillErase -= Time.deltaTime;
        }
        else
        {
            ResetOven();
        }
    }
    #endregion

    #region UI
    public void SetUIActive() 
    {
        if (ovenLocked) { return; }
        hud.gameObject.SetActive(true);
    }
    public void SetUIInActive()
    {
        hud.gameObject.SetActive(false);
    }
    private void SetUiPosition()
    {
        hudTR.position = Camera.main.WorldToScreenPoint(transform.position + hudOffset);

    }
    #endregion
}
