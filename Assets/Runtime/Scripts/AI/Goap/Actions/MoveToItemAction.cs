using Collectible;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class MoveToItemAction : GoapAction
{
    LevelGraph.Node previousNode = null;
    ItemHelper targetedItem = null;
    ExcludeObjectCondition<ItemHelper> excludeObjectCondition;

    public MoveToItemAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"Flee: {combatant.playerName}", false);
        AddPrerquisites($"InventoryFull: {combatant.playerName}", false);
        AddPrerquisites($"HasRecipe: {combatant.playerName}", true);
        AddPrerquisites($"HasBomb: {combatant.playerName}", false);
        AddPrerquisites("ItemAvailable", true);
        AddConcequece($"ItemInArea: {combatant.playerName}", true);
        excludeObjectCondition += CheckItem;
        

        actionType = ActionType.MoveLocationAction;
    }

    public override void Reset()
    {
        base.Reset();
        targetedItem = null;
        previousNode = null;
    }
    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        if (combatant.inventoryFull) { return false; }
        target = FindCLostestObject<ItemHelper>(ItemManager.instance.items.ToArray(), excludeObjectCondition);


        return nodes != null && !combatant.itemInArea && !combatant.inventoryFull && !combatant.isMoving;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if (targetedItem == null || !targetedItem.gameObject.activeSelf)
        {
            ExitEarly();
            return false;
        }

        //Changes the nodes if the item has moved or is gone
        if (targetedItem.node != previousNode)
        {
            nodes = null;
            GetNodes(targetedItem.transform.position);
            previousNode = targetedItem.node;
        }
        if(nodes == null)
        {
            ExitEarly();
            return false;
        }
        conditionMet = MovePosition();
        
        return conditionMet;
    }
    /// <summary>
    /// Gets the target item and sets the pursuit
    /// </summary>
    /// <returns></returns>
    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }

        target = FindCLostestObject<ItemHelper>(ItemManager.instance.items.ToArray(), excludeObjectCondition);
       


        targetedItem = goapAgent.ConvertTarget<ItemHelper>();
        
        //Makes sure the item isnt null
        if(targetedItem == null)
        {
            ExitEarly();
            return false;
        }
        previousNode = targetedItem.node;
        if (targetedItem.targetdBy == null)
        {
            targetedItem.targetdBy = combatant;
        }

        return true;
    }

    private bool CheckItem(ItemHelper item)
    {
        return (item.targetdBy != null) || (!goapAgent.agentsRecipe.ingredients.Contains("") && !GameManager.instance.recipes.CheckIfItemNeeded(goapAgent.agentsRecipe, item, combatant.inventoryManager.itemNames));
    }
}
