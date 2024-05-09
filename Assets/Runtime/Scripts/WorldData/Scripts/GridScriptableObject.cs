using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Level", fileName = "Grid/Level")]
public class GridScriptableObject : ScriptableObject
{
    public LevelGraph grid;
    public GameObject level;

}
#if UNITY_EDITOR
[CustomEditor(typeof(GridScriptableObject))]
public class GridScriptableObjectEditor : Editor
{
    GridScriptableObject m_Grid;
    public override void OnInspectorGUI()
    {
        m_Grid = (GridScriptableObject)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Save"))
        {
            m_Grid.grid.Init();
        }
    }
}
#endif