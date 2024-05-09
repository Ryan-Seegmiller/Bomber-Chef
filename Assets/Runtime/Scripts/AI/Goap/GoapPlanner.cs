using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;

public static class GoapPlanner
{
   public class Node
    {
        public Node parent;
        public float cumulitiveCost;
        //to accomidate value types, we will use the base class object for values
        public HashSet<WorldState> state;
        public GoapAction action;

        public Node(Node parent, float cumulitiveCost, HashSet<WorldState> state, GoapAction action)
        {
            this.parent = parent;
            this.cumulitiveCost = cumulitiveCost;
            this.state = state;
            this.action = action;
        }

    }
    public static Queue<GoapAction> Plan(GameObject agent, HashSet<GoapAction> availableActions, HashSet<WorldState> worldState, HashSet<WorldState> goal)
    {
        foreach(GoapAction action in availableActions)
        {
            action.Reset();
        }

        //check for actions that are able to be preformed
        HashSet<GoapAction> usableActions = new HashSet<GoapAction>();

        foreach (GoapAction actionOption in availableActions)
        {
            if (actionOption.CheckProceduralPrerequisites(agent))
            {
                usableActions.Add(actionOption);
            }
            //discard all the crazy plans that cant be done
        }

        //Record all the final actions or steps that result in desired goal
        List<Node> leaves = new List<Node>();

        Node startNode = new Node(null, 0, worldState, null);
        
        bool success = BuildGraph(startNode, leaves, usableActions, goal);

        if (!success)
        {
           
            //Tis but a pipe-dream
            return null;
        }
        Node cheapestLeaf = null;
        float cheapestValue = float.MaxValue;
        foreach(Node leaf in leaves)
        {
            if(leaf.cumulitiveCost < cheapestValue)
            {
                cheapestValue = leaf.cumulitiveCost;
                cheapestLeaf = leaf;
            }
        }

        List<GoapAction> result = new List<GoapAction>();
        Node current = cheapestLeaf;
        while (current != null)
        {
            if (current.action != null)
            {
                //add it to the front
                result.Insert(0,current.action);
            }
            current = current.parent;
        }
        //we now a valid path to our goal!

        Queue<GoapAction> thePlan = new Queue<GoapAction>();

        foreach (GoapAction action in result)
        {
            thePlan.Enqueue(action);
        }

        return thePlan;
    }
    private static bool BuildGraph(Node parent, List<Node> leaves, HashSet<GoapAction> usableActions, HashSet<WorldState> goal)
    {
        bool foundValidSequence = false;

        foreach(GoapAction action in usableActions)
        { //based on the parent state and the preconditions met?
            if (InState(action.prerequisites, parent.state))
            {
                HashSet<WorldState> currentState = PopulateStates(parent.state, action.concequences);
                
                Node node = new Node(parent, parent.cumulitiveCost + action.cost, currentState, action);

               
                if(InState(goal, currentState))
                {
                    // Weve reached the desired state
                    leaves.Add(node);
                    foundValidSequence = true;
                }
                else
                {
                    //no path has been found so check other branches
                    HashSet<GoapAction> subset = ActionSubset(usableActions, action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                    {
                        foundValidSequence = true;
                    }
                }
            }
        }
        return foundValidSequence;
    }

    private static HashSet<GoapAction> ActionSubset(HashSet<GoapAction> usableActions, GoapAction action)
    {
        HashSet<GoapAction> subset = new HashSet<GoapAction>();
        foreach (GoapAction goapAction in usableActions)
        {
            if (!goapAction.Equals(action))
            {
                subset.Add(goapAction);
            }
        }
        return subset;
    }

    private static HashSet<WorldState> PopulateStates(HashSet<WorldState> currentState, HashSet<WorldState> changeState)
    {
        HashSet<WorldState> state = new HashSet<WorldState>();

        foreach(WorldState pair in currentState)
        {
            state.Add(new WorldState(pair.Key, pair.Value));
        }
        foreach(WorldState change in changeState)
        {   //if the key already exsists in the current state we can update the value
            bool exsists = false;

            foreach(WorldState s in state)
            {
                if (s.Key.Equals(change.Key))
                {
                    exsists = true;
                    break;
                }
            }
            if (exsists) 
            {
                state.RemoveWhere(
                (WorldState kvp) => 
                { 
                    return kvp.Key.Equals(change.Key); 
                });

                state.Add(change);
            }
            else
            {
                state.Add(change);
            }
        }
        return state;
    }

    private static bool InState(HashSet<WorldState> testCondition, HashSet<WorldState> state)
    {
        bool allConditionsMet = true;
        if(testCondition.Count == 0) { allConditionsMet = false; }//Makes sure that the goals arent autocompleted
        foreach(WorldState condition in testCondition)
        {
            bool match = false;
            foreach(WorldState stateCondition in state)
            {   //Only one state needs to be true for the entire thing to be true
                if (stateCondition.Equals(condition))
                {
                    match = true;
                    break;
                }
            }
            if (!match)
            {
                allConditionsMet = false;
            }
        }
        return allConditionsMet;
        
    }
}
