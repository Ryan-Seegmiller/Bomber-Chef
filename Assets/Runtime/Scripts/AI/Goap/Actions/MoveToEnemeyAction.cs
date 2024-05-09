using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class MoveToEnemeyAction : GoapAction
{
    CombatantActions targetCombatant = null;
    LevelGraph.Node previousNode = null;
    ExcludeObjectCondition<CombatantActions> excludeObjectCondition = null;

    public MoveToEnemeyAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"Flee: {combatant.playerName}", false);
        AddPrerquisites($"HasBomb: {combatant.playerName}", true);
        AddConcequece($"CanAim: {combatant.playerName}", true);
        excludeObjectCondition += CheckCombatant;
        actionType = ActionType.MoveLocationAction;
    }


    public override void Reset()
    {
        base.Reset();
        targetCombatant = null;
        previousNode = null;
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        FindCLostestObject<CombatantActions>(GameManager.instance.combatants, excludeObjectCondition);

        return nodes != null && !combatant.isMoving;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();

        //Accounts for if the combanta has moved
        if (targetCombatant.node != previousNode)
        {
            nodes = null;
            previousNode = targetCombatant.node;
            GetNodes(targetCombatant.transform.position, true);
        }
        //Determine if the bomb needs to be dropped
        if (DetermineRun() || !combatant.hasBomb)
        {
            ExitEarly();
            return false;
        }
        //Determines if the bomb can be thrown
        if (DetermineInRange())
        {
            nodes = null;

            GoapBlackboard.LogState(new WorldState($"CanAim: {combatant.playerName}", true));
            return true;
        }
        

        conditionMet = MovePosition();
        
        return conditionMet;
    }
    protected override void ActionPerformed()
    {
        base.ActionPerformed();
        combatant.DropBomb();
        goapAgent.PickState();
    }
    protected override void ExitEarly()
    {
        base.ExitEarly();

        combatant.DropBomb();
        GoapBlackboard.LogState(new WorldState($"Flee: {combatant.playerName}", true));
        GoapBlackboard.LogState(new WorldState($"HasBomb: {combatant.playerName}", false));
        goapAgent.stateMachine.SetState(AIStates.Flee);
    }

    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }

        target = GetMostEfficentCombatant();
        
        targetCombatant = goapAgent.ConvertTarget<CombatantActions>();

        if (targetCombatant != null)
        {
            previousNode = targetCombatant.node;
            goapAgent.targetCombatant = targetCombatant;
        }
        else
        {
            ExitEarly();
            return false;
        }

        return true;
    }

    private bool DetermineInRange()
    {
        bool inRange = Vector3.Distance(combatant.transform.position, targetCombatant.transform.position) < combatant.stregnthClamp.y;
        bool bombReady = combatant.bombHeld.timeRemaining < combatant.bombHeld.explosionTime / 2;

        return inRange && bombReady;
    }
    private bool DetermineRun()
    {
        if(combatant.bombHeld == null) { return false; }
        if(combatant.bombHeld.timeRemaining < combatant.bombHeld.explosionTime / 4)
        {
            return true;
        }
        return false;
    }
    private bool CheckCombatant(CombatantActions combatant)
    {
        return (combatant == this.combatant || combatant.isDamaged || combatant.isDead || combatant == null);
    }
    /// <summary>
    /// Sorts the combatnats by highest effeincey
    /// </summary>
    /// <returns></returns>
    private CombatantActions GetMostEfficentCombatant()
    {
        nodes = null;
        CombatantActions mostThreateningTarget = null;
        float mostEfficent = float.MinValue;
        foreach (CombatantActions combatant in GameManager.instance.combatants)
        {
            if (CheckCombatant(combatant)) { continue; }
            float combatnatDeaths = ((float)GameManager.instance.gameRules.health - (float)combatant.health.health);
            // Adding 1 to each of these that way the effiency is never zero
            if (combatnatDeaths == 0) { combatnatDeaths = 1; }
            float kDRatio = ((float)combatant.killCount + 1) / combatnatDeaths;

            float distance =  Pathfinding.instance.FindPath(this.combatant.transform.position, combatant.transform.position, true).Count;

            float efficecy = kDRatio / distance;

            if(mostEfficent < efficecy)
            {
                mostEfficent = efficecy;
                mostThreateningTarget = combatant;
            }
        }
        GetNodes(mostThreateningTarget.transform.position, true);

        return mostThreateningTarget;
    }
}
