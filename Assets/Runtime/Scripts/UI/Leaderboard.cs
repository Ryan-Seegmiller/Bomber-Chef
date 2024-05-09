using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public LeaderBoardPlayerHelper[] leaderBoardPlayerHelpers = new LeaderBoardPlayerHelper[4];

    public void PopulateLeaderboard(CombatantActions[] combatants)
    {
        CombatantActions[] sortedCombatants = SortCombatants(combatants);
        for (int i = 0; i < sortedCombatants.Length; i++)
        {
            leaderBoardPlayerHelpers[i].nameText.text = sortedCombatants[i].playerName;
            leaderBoardPlayerHelpers[i].killsText.text = sortedCombatants[i].killCount + "K";
            leaderBoardPlayerHelpers[i].deathsText.text = (GameManager.instance.gameRules.health - sortedCombatants[i].GetComponent<Health>().health) + "D";
        }


    }
    private CombatantActions[] SortCombatants(CombatantActions[] combatants)
    {
        CombatantActions[] sortedCombatants = new CombatantActions[combatants.Length];
       
        for(int i = 0; i < combatants.Length; i++) 
        {
            int highestKillCount = 0;
            CombatantActions mostEfficentCombatant = null;
            foreach (CombatantActions combatant in combatants)
            {
                if (combatant.killCount >= highestKillCount && !sortedCombatants.Contains(combatant))
                {
                    highestKillCount = combatant.killCount;
                    mostEfficentCombatant = combatant;
                }
            }
            sortedCombatants[i] = mostEfficentCombatant;
        }
        return sortedCombatants;
    }
}
