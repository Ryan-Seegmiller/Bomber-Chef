using Collectible;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


//TODO: For Rob Goap Agent
[RequireComponent(typeof(CombatantActions))]
public class GoapAgent : MonoBehaviour
{
    #region Declerations

    #region Goap
    private HashSet<GoapAction> availabeleActions = new HashSet<GoapAction>();//Stores all the actions on the player
    private Queue<GoapAction> currentPlan = new Queue<GoapAction>();//Current plan
    private GoapAction currentAction = null;
    private bool hasActionPlan
    {
        get { return (currentPlan != null) ? currentPlan.Count > 0 : false; }
    }

    private HashSet<WorldState> goals = new HashSet<WorldState>();
    #endregion

    [HideInInspector] public Recipe agentsRecipe = null;//Curent Recipe
    [HideInInspector] public CombatantActions combatant;//Combatant thats attached to the AI
    [HideInInspector] public CombatantActions targetCombatant;//Combatant to attack
    [HideInInspector] public bool exitPlan = false;//Force exits the plan in action

    [HideInInspector] public object target;
    [HideInInspector] public List<LevelGraph.Node> nodes = new List<LevelGraph.Node>();

    #region Durations
    public AITimerData timerData = new AITimerData();

    [SerializeField, Range(0, 100)] int wanderChance = 50;
    #endregion

    #region StateMachine
    public FiniteStateMachine stateMachine = new FiniteStateMachine();
        
    public AIStates currentState
    {
        get => stateMachine.state;
        set => stateMachine.SetState(value);
    }

    #region Actions
    public HashSet<GoapAction> wanderActions = new HashSet<GoapAction>();
    public HashSet<GoapAction> gatherActions = new HashSet<GoapAction>();
    public HashSet<GoapAction> bakeActions = new HashSet<GoapAction>();
    public HashSet<GoapAction> attackActions = new HashSet<GoapAction>();
    public HashSet<GoapAction> fleeActions = new HashSet<GoapAction>();
    public HashSet<GoapAction> damagedActions = new HashSet<GoapAction>();
    public HashSet<GoapAction> deadActions = new HashSet<GoapAction>();
    #endregion

    #endregion

    #region Editor
    #if UNITY_EDITOR
    [Header("DEBUG")]
    public bool showPath = false;
    #endif
    #endregion

#endregion

    #region Setup
    private void Awake()
    {
        combatant = GetComponent<CombatantActions>();
        //Sets all the delegates to the state machine
        stateMachine.SetDelegates(StateWander,
        StateGather,
        StateBake,
        StateAttack,
        StateFlee,
        StateDamaged,
        StateDead);

        combatant.agent = this;

        
    }
    private void Start()
    {
        ResetWorldStates();

        InitalizeActionLists();

        //Sets thge initial state
        currentState = AIStates.Wander;

    }
    private void InitalizeActionLists()
    {
        #region Wander Actions
        WanderAction wanderAction = new WanderAction(this, timerData.wanderIdleDuration);
        wanderActions.Add(wanderAction);

        #endregion

        #region GatherActions
        PickRecipeAction pickRecipeAction = new PickRecipeAction(this, timerData.pickRecipeDuration);
        MoveToItemAction moveToItemAction = new MoveToItemAction(this);
        PickupAction pickupAction = new PickupAction(this, timerData.pickupItemDuration);

        gatherActions.Add(pickRecipeAction);
        gatherActions.Add(moveToItemAction);
        gatherActions.Add(pickupAction);

        #endregion

        #region Bake
        MoveToOvenAction moveToOvenAction = new MoveToOvenAction(this);
        PlaceItemAction placeItemAction = new PlaceItemAction(this, timerData.placeItemDuration);

        bakeActions.Add(moveToOvenAction);
        bakeActions.Add(placeItemAction);

        #endregion

        #region Attack
        //Cretaes the actions
        MoveToBombAction moveToBombAction = new MoveToBombAction(this);
        PickUpBombAction pickUpBombAction = new PickUpBombAction(this, timerData.pickupBombDuration);
        MoveToEnemeyAction moveToEnemeyAction = new MoveToEnemeyAction(this);
        AimingAction aimingAction = new AimingAction(this, timerData.aimingDuration);
        AttackAction attackAction = new AttackAction(this, timerData.attackDuration);
        DropBombAction dropBombAction = new DropBombAction(this, timerData.dropBombDuration);

        //Adds to the list
        attackActions.Add(moveToBombAction);
        attackActions.Add(pickUpBombAction);
        attackActions.Add(moveToEnemeyAction);
        attackActions.Add(aimingAction);
        attackActions.Add(attackAction);
        //TODO: Itearate on this idea further in the future
        //attackActions.Add(dropBombAction);

        #endregion

        #region FleeAction
        FleeAction fleeAction = new FleeAction(this, timerData.fleeDuration);

        fleeActions.Add(fleeAction);
        #endregion

    }
    public void ResetWorldStates()
    {
        GoapBlackboard.LogState(new WorldState($"HasRecipe: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"AtOven: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"InventoryFull: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"HasBomb: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"AtBomb: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"Flee: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"IsAiming: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"Flee: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"CanWander: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"BombDone: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"CanAim: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"HasItem: {combatant.playerName}", false));
        ResetNodes();

    }
    public void ResetNodes()
    {
        nodes = null;
    }


    #endregion

    private void Update()
    {
        if (GameManager.instance.gameState != GameState.Playing) { return; }

        if (combatant.isDamaged || combatant.isDead) { return; }
        ExectutePlan();

        if (!hasActionPlan || exitPlan)
        {
            MakePlan();
        }

    }

    #region State Methods
    /// Each of these clears the goals list and quits out of any plan currently executuing 
    /// then sets a new goal and world state if needed
   
    public void BaseStateChange()
    {
        goals.Clear();
        ForceQuitPlan();
    }

    public void StateWander()
    {
        BaseStateChange();
        GoapBlackboard.LogState(new WorldState($"CanWander: {combatant.playerName}", true));
        goals.Add(new WorldState($"CanWander: {combatant.playerName}", false));
        availabeleActions = wanderActions;
    }

    public void StateGather()
    {
        BaseStateChange();
        goals.Add(new WorldState($"HasItem: {combatant.playerName}", true));
        availabeleActions = gatherActions;
    }

    public void StateBake()
    {
        BaseStateChange();
        goals.Add(new WorldState("ItemPlaced", true));
        availabeleActions = bakeActions;
    }

    public void StateAttack()
    {
        BaseStateChange();
        goals.Add(new WorldState($"Attacked: {combatant.playerName}", true));
        availabeleActions = attackActions;
    }

    public void StateFlee()
    {

        BaseStateChange();
        goals.Add(new WorldState($"Flee: {combatant.playerName}", false));
        availabeleActions = fleeActions;

    }

    public void StateDamaged()
    {
        BaseStateChange();
        ResetNodes();
        ResetWorldStates();
        availabeleActions = damagedActions;
    }

    public void StateDead()
    {
        BaseStateChange();
        availabeleActions = deadActions;
    }
    

    //At this point i have no clear direction for this state maching so this is what im doing
    public void PickState()
    {
        
        if(currentState != AIStates.Attack)
        { 
            StateAttack();
            if(CheckForPlan())
            {
                stateMachine.SetState(AIStates.Attack);
                return;
            }
        }
        StateBake();
        if (CheckForPlan())
        {
            stateMachine.SetState(AIStates.Bake);
            return;
        }
        
        //Random pick for wander or gathering
        if (!WanderOrGather())
        {
            stateMachine.SetState(AIStates.Wander);
            return;
        }
        
    }
    bool WanderOrGather()
    {
        AIStates state = AIStates.Gather;
        if (GameManager.instance.combatantsLeft > 2)
        {
            state = (UnityEngine.Random.Range(0, 100) > wanderChance) ? AIStates.Gather : AIStates.Wander;
        }
        
        
        if(state == AIStates.Gather)
        {
            StateGather();
            if (CheckForPlan())
            {
                stateMachine.SetState(AIStates.Gather);
                return true;
            }
            return false;
        }
        stateMachine.SetState(AIStates.Wander);
        return true;

    }
    #endregion

    #region Planning
    /// <summary>
    /// Checks the plan if its valid
    /// </summary>
    /// <returns>True or false depending on if the plan can be executed</returns>
    bool CheckForPlan()
    {
        currentPlan = GoapPlanner.Plan(gameObject, availabeleActions, GoapBlackboard.stateOfTheWorld, goals);

        return (currentPlan != null) ? currentPlan.Count > 0 : false;

    }
    /// <summary>
    /// Makes a plan based off the available goals and actions 
    /// </summary>
    void MakePlan()
    {
        currentPlan = GoapPlanner.Plan(gameObject, availabeleActions, GoapBlackboard.stateOfTheWorld, goals);

        if(!hasActionPlan && currentState == AIStates.Attack)
        {
            PickState();
        }
    }
    /// <summary>
    /// Executes the plan every frame
    /// </summary>
    void ExectutePlan()
    {
        if (!hasActionPlan) { return; }

        currentAction = currentPlan.Peek();
        if (currentAction.Perform(gameObject))
        {
            NextAction();                
        }
    }
    /// <summary>
    /// Gets the next action
    /// </summary>
    private void NextAction()
    {
        if (currentPlan == null) { return; }

        currentPlan.Dequeue();
        if (currentPlan.Count != 0)
        {
            currentAction = currentPlan.Peek();
        }
    }
    void ForceQuitPlan()
    {
        currentPlan = null;

        if (combatant.isAiming)
        {
            combatant.isAiming = false;
        }
    }
    #endregion

    void OnFlee()
    {
        if(currentState == AIStates.Flee || currentState == AIStates.Damaged || currentState == AIStates.Dead) { return; }
        if (combatant.hasBomb) { combatant.DropBomb(); }
        GoapBlackboard.LogState(new WorldState($"Flee: {combatant.playerName}", true));
        currentState = AIStates.Flee;

    }

    public T ConvertTarget<T>()
    {
        return (T)target;
    }
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (showPath && nodes != null)
        {
            Vector3 yPos = new Vector3(0, transform.position.y, 0);
            Gizmos.color = Color.white;
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Gizmos.DrawLine(GameManager.instance.grid.GetWorldPositionCenter(nodes[i].location) + yPos, GameManager.instance.grid.GetWorldPositionCenter(nodes[i + 1].location) + yPos);

            }
        }
    }
#endif

}
[System.Serializable]
public struct AITimerData
{
    public float pickRecipeDuration;
    public float pickupItemDuration;
    public float placeItemDuration;
    public float pickupBombDuration;
    public float dropBombDuration;
    public float aimingDuration ;
    public float attackDuration;
    public float wanderIdleDuration;
    public float fleeDuration;
}
