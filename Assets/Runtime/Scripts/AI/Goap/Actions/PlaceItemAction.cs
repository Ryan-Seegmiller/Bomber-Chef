using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class PlaceItemAction : GoapAction
{
    public PlaceItemAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"AtOven: {combatant.playerName}", true);
        AddPrerquisites($"HasItem: {combatant.playerName}", true);
        AddConcequece($"ItemPlaced", true);
        actionType = ActionType.PickupAction;
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
            goapAgent.PickState();
        }
        return conditionMet;
    }


    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }
        if (combatant.inventoryFull)
        {   //Log false if inventory was full
            GoapBlackboard.LogState(new WorldState($"InventoryFull: {combatant.playerName}", false));
        }
        combatant.PlaceItem();//Places the item
       
        if (combatant.inventoryManager.itemNames.Count <= 0)
        {   //Logs a completed recipe
            GoapBlackboard.LogState(new WorldState($"HasItem: {combatant.playerName}", false));
            GoapBlackboard.LogState(new WorldState($"HasRecipe: {combatant.playerName}", false));
        }

        return true;
    }

}
