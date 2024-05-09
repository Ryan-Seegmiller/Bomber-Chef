using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class MoveToBombAction : GoapAction
{
    BaseBomb targetBomb = null;
    ExcludeObjectCondition<BaseBomb> objectCondition;
    LevelGraph.Node previosNode = null;

    public MoveToBombAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"Flee: {combatant.playerName}", false);
        AddPrerquisites($"HasBomb: {combatant.playerName}", false);
        AddPrerquisites($"AtBomb: {combatant.playerName}", false);
        AddConcequece($"AtBomb: {combatant.playerName}", true);
        objectCondition += CheckBomb;
        actionType = ActionType.MoveLocationAction;
    }
    public override void Reset()
    {
        base.Reset();
        targetBomb = null;
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {

        FindCLostestObject<BaseBomb>(GameManager.instance.bombs.ToArray(), objectCondition);

        return nodes != null;

    }
    public override bool Perform(GameObject agent)
    {
        ActionStart();
        if(targetBomb == null || !targetBomb.isActiveAndEnabled || GameManager.instance.bombs.Count <= 0) 
        {
            ExitEarly();
            return false;
        }
        if(targetBomb.node != previosNode)
        {
            nodes = null;
            previosNode = targetBomb.node;
            GetNodes(targetBomb.transform.position, true);
        }
        if (nodes == null)
        {
            ExitEarly();
            return false;
        }

        conditionMet = MovePosition();
        
        return conditionMet;
    }
    protected override void ActionPerformed()
    {
        base.ActionPerformed();
        GoapBlackboard.LogState(new WorldState($"AtBomb: {combatant.playerName}", true));
    }
    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }

        target = FindCLostestObject<BaseBomb>(GameManager.instance.bombs.ToArray(), objectCondition, true);

        targetBomb = goapAgent.ConvertTarget<BaseBomb>();

        if(targetBomb == null)
        {
            ExitEarly();
            return false;
        }

        targetBomb.targetdBy = combatant;
        previosNode = targetBomb.node;

        return true;
    }
    /// <summary>
    /// Checks indiviulal bomb for eligibality of pickup
    /// </summary>
    /// <param name="bomb"></param>
    /// <returns></returns>
    private bool CheckBomb(BaseBomb bomb)
    {
        return ((bomb.GetThrower() == combatant) || ((bomb.ignited && bomb.timeRemaining < bomb.explosionTime / 4)) || (bomb.targetdBy != null && bomb.targetdBy != combatant));
    }
}
