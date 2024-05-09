using Collectible;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

using WorldState = System.Collections.Generic.KeyValuePair<string, object>;


//TODO: For Rob GoapBlackBoard
public class GoapBlackboard : MonoBehaviour
{
    #region Decleartions
    public static GoapBlackboard instance;

    [SerializeField] private TextMeshProUGUI display;
    
    public bool debugAIOn = false;

    public static HashSet<WorldState> stateOfTheWorld = new HashSet<WorldState>(); //World state hashset

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region States
    /// <summary>
    /// Logs states and removes duplicates
    /// </summary>
    /// <param name="w_State"></param>
    public static void LogState(WorldState w_State)
    {
        bool exists = false;
        foreach(WorldState status in stateOfTheWorld)
        {
            if(w_State.Key.Equals(status.Key))
            {
                exists = true; 
                break;
            }
        }
        if (exists)
        {
            stateOfTheWorld.RemoveWhere((WorldState kvp) => kvp.Key.Equals(w_State.Key));
        }
        stateOfTheWorld.Add(w_State);
        if (instance.debugAIOn)
        {
            instance.DisplayState();
        }
    }
    #endregion

    #region Display
    //DEBUG
    //Displays the world states to a console window outside of view
    void DisplayState()
    {
        WorldState[] world = stateOfTheWorld.ToArray();
        string s = "";

        for (int i = 0; i < GameManager.instance.combatants.Length; i++)
        {
            CombatantActions combatant = GameManager.instance.combatants[i];
            if (combatant.isAI)
            {
                s += combatant.playerName + " Sate : " + combatant.agent.currentState + "\n";
            }
        }

        for (int i = 0; i < world.Length; i++)
        {
            s += world[i].Key + " : " + ((world[i].Value == null) ? "null" : world[i].Value.ToString()) + " \n";
        }

        display.text = s;
    }
    //DEBUG
    #endregion
}
