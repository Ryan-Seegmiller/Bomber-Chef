using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class DropBombAction : GoapAction
{
    public DropBombAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"HasBomb: {combatant.playerName}", true);
        AddConcequece($"HasBomb: {combatant.playerName}", false);
        AddConcequece($"Attacked: {combatant.playerName}", true);

    }

    public override void Reset()
    {
        base.Reset();
    }
    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        bool combatantInArea = Physics.OverlapSphere(combatant.transform.position, combatant.ineractRadius / 2, 1 << combatant.gameObject.layer).Length > 1;
        return combatantInArea;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if((Time.time - startTime) > duration)
        {
            combatant.DropBomb();
            conditionMet = true; 
        }
        return conditionMet;
    }
    protected override void ActionPerformed()
    {
        base.ActionPerformed();
        GoapBlackboard.LogState(new WorldState($"HasBomb: {combatant.playerName}", false));
    }
}
