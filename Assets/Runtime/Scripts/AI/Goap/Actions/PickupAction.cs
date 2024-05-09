using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;

public class PickupAction : GoapAction
{
    public PickupAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"ItemInArea: {combatant.playerName}", true);
        AddConcequece($"HasItem: {combatant.playerName}", true);
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();
        if((Time.time - startTime) > duration)
        {
            conditionMet = true;
        }
        return conditionMet;
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return !combatant.inventoryFull;
    }

    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }

        //Makes sure the item in the correct item and there actually is an item
        if ((combatant.itemInArea && !GameManager.instance.recipes.CheckIfItemNeeded(goapAgent.agentsRecipe, combatant.closestItem, combatant.inventoryManager.itemNames)) || !combatant.itemInArea)
        {
            ExitEarly();
            return false;
        }
        combatant.PickupItem();
        GoapBlackboard.LogState(new WorldState($"HasItem: {combatant.playerName}", true));
        if (combatant.inventoryFull)
        {
            GoapBlackboard.LogState(new WorldState($"InventoryFull: {combatant.playerName}", true));
            GoapBlackboard.LogState(new WorldState($"ItemInArea: {combatant.playerName}", false));
            goapAgent.currentState = AIStates.Bake;
        }
        return true;
    }
    protected override void ExitEarly()
    {
        base.ExitEarly();
        GoapBlackboard.LogState(new WorldState($"ItemInArea: {combatant.playerName}", false));
    }
}
