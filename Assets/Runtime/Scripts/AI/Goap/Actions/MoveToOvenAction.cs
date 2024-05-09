using System.Collections.Generic;
//wtf is this?
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class MoveToOvenAction : GoapAction
{
    private Oven targetOven = null;
    private ExcludeObjectCondition<Oven> excludeObjectCondition = null;

    public MoveToOvenAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"Flee: {combatant.playerName}", false);
        AddPrerquisites($"InventoryFull: {combatant.playerName}", true);
        AddPrerquisites($"AtOven: {combatant.playerName}", false);
        AddConcequece($"AtOven: {combatant.playerName}", true);
        excludeObjectCondition += CheckOven;
        actionType = ActionType.MoveLocationAction;
    }

    public override void Reset()
    {
        base.Reset();
        targetOven = null;
    }
    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        FindCLostestObject<Oven>(GameManager.instance.ovens, excludeObjectCondition);

        return nodes != null && !combatant.isMoving;
    }
    /// <summary>
    /// Perfoms the action
    /// </summary>
    /// <param name="agent"></param>
    /// <returns></returns>
    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if(nodes == null)
        {
            goapAgent.PickState();

            return false;
        }
        conditionMet = MovePosition();

        return conditionMet;
    }

    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }

        target = FindCLostestObject<Oven>(GameManager.instance.ovens, excludeObjectCondition);

        targetOven = goapAgent.ConvertTarget<Oven>();

        //Makes sure that the oven isnt already being targeted
        if(targetOven.targetdBy == null)
        {
            targetOven.targetdBy = combatant;
        }
        return true;
    }

    protected override void ActionPerformed()
    {
        GoapBlackboard.LogState(new WorldState($"AtOven: {combatant.playerName}", true));
    }

    private bool CheckOven(Oven oven)
    {
        return (oven.combatantInUse != null && oven.combatantInUse != combatant) || (oven.targetdBy != null) || oven.ovenLocked;
    }

}