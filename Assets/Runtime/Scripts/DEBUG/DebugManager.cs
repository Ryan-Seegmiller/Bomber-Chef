using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
public class DebugManager : MonoBehaviour
{
    #region Decleartions
    #region AI
    [Header("AI Debug")]
    public AgentDebugData[] agents = new AgentDebugData[0];
    #endregion

    #region Level
    [Header("Level")]
    public bool showLevelLayout = false;
    public bool showSpawnPoints = false;
    #endregion

    #region Combatants
    [Header("Combatants")]
    public bool showInteractRadius = false;
    #endregion

    #region Bombs
    [Header("Bombs")]
    public bool showBombEsplosionRadius = false;
    #endregion

    #endregion

    #region Setup
    public void OnSetup()
    {
        SetupAIData();
    }
    private void SetupAIData()
    {
        agents = new AgentDebugData[GameManager.instance.combatants.Length - 1];
        int i = 0;
        foreach (CombatantActions combatant in GameManager.instance.combatants)
        {
            if (!combatant.isAI) { continue; }
            agents[i++] = new AgentDebugData(combatant.playerName, combatant.GetComponent<GoapAgent>().showPath);
        }
    }


#endregion

    public void OnValidate()
    {
        int offset = 0;
        for(int i = 0; i < agents.Length; i++)
        {
            CombatantActions currentCombatant = GameManager.instance.combatants[i];
            if (!currentCombatant.isAI)
            {
                offset++;
                continue;
            }

            currentCombatant.GetComponent<GoapAgent>().showPath = agents[i - offset].showPath;            
        }
        
    }

}
[System.Serializable]
public struct AgentDebugData
{
    public string agentName;
    public bool showPath;

    public AgentDebugData(string agentName, bool showPath)
    {
        this.agentName = agentName;
        this.showPath = showPath;
    }
}
#endif

