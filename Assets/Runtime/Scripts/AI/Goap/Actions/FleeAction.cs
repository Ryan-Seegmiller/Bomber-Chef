using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class FleeAction : GoapAction
{
    Vector3? fleePosition = null;

    public FleeAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"Flee: {combatant.playerName}", true);
        AddPrerquisites($"HasBomb: {combatant.playerName}", false);

        AddConcequece($"Flee: {combatant.playerName}", false);

        actionType = ActionType.MoveLocationAction;
    }

    public override void Reset()
    {
        base.Reset();
        fleePosition = null;
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return true;
    }    

    public override bool Perform(GameObject agent)
    {

        ActionStart();
        
        if(nodes == null)
        {
            ExitEarly();
            return false;
        }
        if ((Time.time - startTime) > duration)
        {
            conditionMet = MovePosition();
        }

        return conditionMet;
    }
    protected override void ActionPerformed()
    {
        base.ActionPerformed();
        GoapBlackboard.LogState(new WorldState($"Flee: {combatant.playerName}", false));
        goapAgent.PickState();
    }

    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }

        fleePosition = Pathfinding.instance.GetRandomPosition();
        nodes = null;

        GetNodes(fleePosition, true);

        return true;
    }
}
