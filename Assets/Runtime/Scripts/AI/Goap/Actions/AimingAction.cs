using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class AimingAction : GoapAction
{
    LevelGraph.Node previousNode = null; 
    private Vector3 previousDirection = Vector3.zero;

    public AimingAction(GoapAgent agent, float duration = 0f) : base (agent, duration)
    {
        AddPrerquisites($"HasBomb: {combatant.playerName}", true);
        AddPrerquisites($"CanAim: {combatant.playerName}", true);
        AddPrerquisites($"IsAiming: {combatant.playerName}", false);
        AddConcequece($"IsAiming: {combatant.playerName}", true);
        actionType = ActionType.ThrowAction;
    }
    public override void Reset()
    {
        base.Reset();
        previousNode = null;
        previousDirection = Vector3.zero;
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return true;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if (previousNode != goapAgent.targetCombatant.node)
        {
            previousNode = goapAgent.targetCombatant.node;
        }

        combatant.DrawProjection(); // draws the projection

        if(goapAgent.targetCombatant.transform.position - combatant.transform.position == Vector3.zero)
        {
            combatant.Throw();
        }
        //Lerps the direction of the forward to look at the target
        Vector3 newDirection = (goapAgent.targetCombatant.transform.position - combatant.transform.position).normalized;
        if(newDirection != Vector3.zero)
        {
            combatant.transform.forward = Vector3.Lerp(previousDirection, newDirection, 1 - ((duration - (Time.time - startTime) / duration)));
        }

        if (DetermineThrowEarly())
        {
            combatant.Throw();
            ExitEarly();
            return true;

        }
        if(combatant.bombHeld == null)
        {
            ExitEarly();
            return false;
        }

        if ((Time.time - startTime) > duration)
        {
            combatant.transform.forward = newDirection;
            conditionMet = true;
        }

        return conditionMet;
    }
    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }
        combatant.isAiming = true;
        previousDirection = combatant.transform.forward;

        combatant.bombThrowStrength = Vector3.Distance(combatant.transform.position, goapAgent.targetCombatant.transform.position);
        combatant.bombThrowStrength = Mathf.Clamp(combatant.bombThrowStrength, combatant.stregnthClamp.x, combatant.stregnthClamp.y);

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
        GoapBlackboard.LogState(new WorldState($"IsAiming: {combatant.playerName}", true));
    }

    private bool DetermineThrowEarly()
    {
        if(combatant.bombHeld == null) { return false; }
        if (combatant.bombHeld.timeRemaining < combatant.bombHeld.explosionTime / 3)
        {
            return true;
        }
        return false;
    }
}
