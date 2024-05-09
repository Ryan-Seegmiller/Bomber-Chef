using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class AttackAction : GoapAction
{
    public AttackAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"IsAiming: {combatant.playerName}", true);
        AddPrerquisites($"HasBomb: {combatant.playerName}", true);

        AddConcequece($"Attacked: {combatant.playerName}", true);

        actionType = ActionType.ThrowAction;
    }

    public override void Reset()
    {
        base.Reset();
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return true;
    }
    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if(combatant.bombHeld == null)
        {
            ExitEarly();
            return false;
        }
     
        if ((Time.time - startTime) > duration) 
        {  
            conditionMet = true;      
        }

        return conditionMet;
    }
    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }

        return true;
    }
    protected override void ExitEarly()
    {
        base.ExitEarly();
        GoapBlackboard.LogState(new WorldState($"CanAim: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"HasBomb: {combatant.playerName}", false));
    }

    protected override void ActionPerformed()
    {
        base.ActionPerformed();
        combatant.Throw();
        combatant.isAiming = false;

        GoapBlackboard.LogState(new WorldState($"CanAim: {combatant.playerName}", false));
        GoapBlackboard.LogState(new WorldState($"HasBomb: {combatant.playerName}", false));

        goapAgent.PickState();
    }
}
