using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Combatants", menuName = "CombatantsHolder")]
public class CombatantsScriptableObject : ScriptableObject
{
    [NonReorderable] public GameObject[] combatants = new GameObject[4];
}
