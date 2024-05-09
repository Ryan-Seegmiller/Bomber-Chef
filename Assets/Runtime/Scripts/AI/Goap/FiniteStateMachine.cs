using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//TODO: For Rob State machine
public class FiniteStateMachine
{
    #region Delegates
    //Contructor that takes in a method
    public void SetDelegates(StateBehviours wander, StateBehviours gather, StateBehviours bake, StateBehviours attack, StateBehviours flee, StateBehviours damaged, StateBehviours dead) 
    { 
        Wander = wander;
        Gather = gather;
        Bake = bake;
        Attack = attack;
        Flee = flee;
        Damaged = damaged;
        Dead = dead;
    }

    public delegate void StateBehviours();

    //Delegates that do the behaviours of the states
    public StateBehviours Wander;
    public StateBehviours Gather;
    public StateBehviours Bake;
    public StateBehviours Attack;
    public StateBehviours Flee;
    public StateBehviours Damaged;
    public StateBehviours Dead;
    #endregion

    public AIStates state = AIStates.Wander;

    //Invokes a delegate based on the state that is being set to
    public void SetState(AIStates state)
    {
        this.state = state;
        switch (state)
        {
            case AIStates.Wander:
                Wander.Invoke();
                break;
            case AIStates.Gather:
                Gather.Invoke();
                break;
            case AIStates.Bake:
                Bake.Invoke();
                break;
            case AIStates.Attack:
                Attack.Invoke();
                break;
            case AIStates.Flee:
                Flee.Invoke();
                break;
            case AIStates.Damaged:
                Damaged.Invoke();
                break;
            case AIStates.Dead:
                Dead.Invoke();
                break;
            default:
                Debug.LogError(state.ToString() + ": is not implemeted");
                break;
        }
    }

}
public enum AIStates
{
    Wander,
    Gather,
    Bake,
    Attack,
    Flee,
    Damaged,
    Dead
}



