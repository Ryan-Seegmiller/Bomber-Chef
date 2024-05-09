using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelHolder", menuName = "Grid/LevelHolder")]
public class LevelsScriptableObject : ScriptableObject
{
    public GridScriptableObject[] levels;
}
