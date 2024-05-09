using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class PickUpBombAction : GoapAction
{
    public PickUpBombAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"AtBomb: {combatant.playerName}", true);
        AddPrerquisites($"HasBomb: {combatant.playerName}", false);
        AddConcequece($"HasBomb: {combatant.playerName}", true);

        actionType = ActionType.PickupAction;
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return true;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if((Time.time - startTime) > duration) 
        {

            if (combatant.closestBomb == null)
            {
                ExitEarly();
                return false;
            }
            //Flee the bomb area
            if (combatant.closestBomb.ignited && combatant.closestBomb.timeRemaining < combatant.closestBomb.explosionTime / 3)
            {
                goapAgent.stateMachine.SetState(AIStates.Flee);
                return false;
            } 
           
            conditionMet = true;
        }


        return conditionMet;
    }
    protected override void ExitEarly()
    {
        GoapBlackboard.LogState(new WorldState($"AtBomb: {combatant.playerName}", false));
        base.ExitEarly();
    }
    protected override void ActionPerformed()
    {
        base.ActionPerformed();

        combatant.PickUpBomb();

        GoapBlackboard.LogState(new WorldState($"HasBomb: {combatant.playerName}", true));
        GoapBlackboard.LogState(new WorldState($"AtBomb: {combatant.playerName}", false));

    }
}
