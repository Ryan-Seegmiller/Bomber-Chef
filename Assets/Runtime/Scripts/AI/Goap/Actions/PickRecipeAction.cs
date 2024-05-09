using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;

public class PickRecipeAction : GoapAction
{

    public PickRecipeAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"HasRecipe: {combatant.playerName}", false);
        AddConcequece($"HasRecipe: {combatant.playerName}", true);
    }
    public override void Reset()
    {
        base.Reset();
        duration = 0;
    }
    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return true;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();
        if ((Time.time - startTime) > duration)
        {
            conditionMet = true;
        }
        return conditionMet;
    }
    protected override void ActionPerformed()
    {
        base.ActionPerformed();
        goapAgent.agentsRecipe = GameManager.instance.GetRandomRecipe();
        GoapBlackboard.LogState(new WorldState($"HasRecipe: {combatant.playerName}", true));
    }

}
