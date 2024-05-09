using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class TossItemAction : GoapAction
{
    public TossItemAction()
    {
        AddConcequece("ThrowItem", false);
    }

    public override void Reset()
    {
        base.Reset();
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return combatant.inventoryManager.itemNames.Count > 0;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if ((Time.time - startTime) > duration)
        {
            conditionMet = true;
        }
        return true;
    }
    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }
        //Tosses item
        combatant.GetRidOfItem();
        if (!combatant.inventoryFull)
        {
            GoapBlackboard.LogState(new WorldState($"InventoryFull: {combatant.playerName}", false));
        }
        if (combatant.inventoryManager.itemNames.Count <= 0)
        {
            GoapBlackboard.LogState(new WorldState($"HasItem: {combatant.playerName}", false));
        }

        return true;
    }
}
