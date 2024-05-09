using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


public class WanderAction : GoapAction
{
    Vector3 wanderPosition = Vector3.zero;
    bool _idleTimer = false;
    bool idleTimer
    {
        get { return _idleTimer; }
        set 
        { 
            if (_idleTimer != value)
            {
                if (value)
                {
                    startTime = Time.time;
                }
                _idleTimer = value; 
            }
        }
    }

    public WanderAction(GoapAgent agent, float duration = 0f) : base(agent, duration)
    {
        AddPrerquisites($"CanWander: {combatant.playerName}", true);
        AddConcequece($"CanWander: {combatant.playerName}", false);
        actionType = ActionType.MoveLocationAction;
    }
    public override void Reset()
    {
        base.Reset();
        wanderPosition = Vector3.zero;
        idleTimer = false;
    }

    public override bool CheckProceduralPrerequisites(GameObject agent)
    {
        return true;
    }

    public override bool Perform(GameObject agent)
    {
        ActionStart();

        if(nodes == null)
        {
            ExitEarly();
            return false;
        }
        if (!idleTimer)
        {
            idleTimer = MovePosition();
        }
        else if((Time.time - startTime > duration)) 
        { 
            conditionMet = true;
        }

        return conditionMet;
    }
    protected override void ActionPerformed()
    {
        base.ActionPerformed();
        GoapBlackboard.LogState(new WorldState($"CanWander: {combatant.playerName}", false));
        goapAgent.PickState();
    }
    public override bool ActionStart()
    {
        if (!base.ActionStart()) { return false; }
        nodes = null;
        wanderPosition = Pathfinding.instance.GetRandomPosition();
        GetNodes(wanderPosition);
        if(nodes == null)
        {
            ExitEarly();
            return false;
        }
       

        return true;

    }
}
