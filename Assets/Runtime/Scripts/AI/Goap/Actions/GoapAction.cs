using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using WorldState = System.Collections.Generic.KeyValuePair<string, object>;

public abstract class GoapAction
{
    #region Setup
    public HashSet<WorldState> prerequisites
    {
        get;
        protected set;
    }

    public HashSet<WorldState> concequences
    {
        get;
        protected set;
    }


    public int cost = 1;
    protected float startTime = 0;
    protected GoapAgent goapAgent;
    [HideInInspector] public List<LevelGraph.Node> nodes
    {
        get => goapAgent.nodes;
        set => goapAgent.nodes = value;
    }
    private bool _conditionMet;
    protected bool conditionMet 
    {
        get { return _conditionMet; }
        set 
        {
            if (value)
            {
                ActionPerformed();
            }
            _conditionMet = value; 
        
        }
    }
    protected object target{
        get => goapAgent.target;
        set => goapAgent.target = value;
    }

    protected ActionType actionType;

    protected float duration;
    #endregion

    protected delegate bool ExcludeObjectCondition<T>(T obj);


    public GoapAction(GoapAgent agent, float duraition = 0f)
    {
        prerequisites = new HashSet<WorldState>();
        concequences = new HashSet<WorldState>();
        
        goapAgent = agent;
        this.duration = duraition;
    }

    protected CombatantActions combatant => goapAgent.combatant;

    /// <summary>
    /// Performs at the start of the perform method....Should call only once
    /// </summary>
    public virtual bool ActionStart()
    {
        if(startTime != 0) { return false;}
        startTime = Time.time;
        return true;
    }

    public GoapAction()
    {
        prerequisites = new HashSet<WorldState> ();
        concequences = new HashSet<WorldState>();
    }
    public virtual void Reset()
    {
        startTime = 0;
        conditionMet = false;
    }

    #region Prerequistses

    public abstract bool CheckProceduralPrerequisites(GameObject agent);


    public void AddPrerquisites(string key, object value)
    {
       AddKeyValuePair(prerequisites, key, value);
    }
    public void RemovePrerequisites(string key)
    {
        RemoveKeyValuePair(prerequisites, key);
    }
    #endregion

    #region Concequenses
    public void AddConcequece(string key, object value)
    {
        AddKeyValuePair(concequences, key, value);
    }
    public void RemoveConcequence(string key)
    {
       RemoveKeyValuePair(concequences, key);
    }
    #endregion

    #region KeyValuePairs
    public void AddKeyValuePair(HashSet<WorldState> hashSet, string key, object value)
    {
        hashSet.Add(new WorldState(key, value));
    }
    public void RemoveKeyValuePair(HashSet<WorldState> hashSet, string key)
    {
        WorldState requirement = default(WorldState);
        foreach (WorldState item in hashSet)
        {
            if (requirement.Key.Equals(item.Key))
            {
                requirement = item;
            }
        }
        if (!default(WorldState).Equals(requirement))
        {
            hashSet.Remove(requirement);
        }
    }
    #endregion

    public bool MovePosition()
    {
        return goapAgent.combatant.GetAIDirection(ref goapAgent.nodes);
    }
    public void GetNodes(Vector3? position, bool ignoreDanger = false)
    {
        if(nodes != null)
        {
            return;
        }
        
        if (position != null)
        {
            nodes = Pathfinding.instance.FindPath(goapAgent.transform.position, (Vector3)position, ignoreDanger || combatant.node.inDanger);
            if (nodes != null)
            {
                nodes.Remove(nodes[nodes.Count - 1]);
            }
        }
    }

    protected T FindCLostestObject<T>(T[] objectArray, ExcludeObjectCondition<T> condition, bool ignoreDanger = false)
    {
        T clostestObject = default;
        List<T> excludedObjs = new List<T>();
        float shortestDistance = float.MaxValue;
        List<LevelGraph.Node> testPath = null;

        while ((testPath == null) && (excludedObjs.Count != objectArray.Length)) 
        {
            for (int i = 0; i < objectArray.Length; i++)
            {
                if (objectArray[i] as MonoBehaviour)
                {
                    if (excludedObjs.Contains(objectArray[i])) { continue; }
                    //Checks if the confdition is met
                    if (condition(objectArray[i])) { continue; }

                    //Determines if the object is closer than the closest object
                    Vector3 position = (objectArray[i] as MonoBehaviour).transform.position;
                    float distantce = Vector3.Distance(goapAgent.transform.position, position);
                    if (distantce < shortestDistance)
                    {
                        shortestDistance = distantce;
                        clostestObject = objectArray[i];
                    }
                }
                else
                {
                    Debug.LogWarning("This object " + typeof(T) + " cannot be converted into a monobehaviour");
                    return default;
                }
            }
            if(clostestObject != null)
            {
                testPath = Pathfinding.instance.FindPath(goapAgent.transform.position, (clostestObject as MonoBehaviour).transform.position, ignoreDanger || combatant.node.inDanger);
                if(testPath == null)
                {   //Reset values and add to the excluded objects
                    shortestDistance = float.MaxValue;
                    excludedObjs.Add(clostestObject);
                    clostestObject = default;
                }
            }
            else
            {
                break;
            }
        }
       
        nodes = testPath;

        return clostestObject;
    }

    public abstract bool Perform(GameObject agent);

    protected virtual void ExitEarly()
    {
        if (actionType == ActionType.MoveLocationAction)
        {
            nodes = null;
        }
        goapAgent.PickState(); 
    }
    protected virtual void ActionPerformed()
    {
        if(actionType == ActionType.MoveLocationAction)
        {
            nodes = null;
        }
    }

    public enum ActionType
    {
        PickupAction,
        MoveLocationAction,
        ThrowAction
    }

}
