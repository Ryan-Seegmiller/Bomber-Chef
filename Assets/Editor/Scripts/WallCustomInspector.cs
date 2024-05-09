using System.Drawing.Printing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(Wall))]
public class WallCustomInspector : Editor
{
    Wall wall;
    Vector3 position = Vector3.zero;
    public override void OnInspectorGUI()
    {
        if(wall == null)
        {
            wall = (Wall)target;
        }

        EditorGUI.BeginChangeCheck();
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((Wall)target), typeof(Wall), false);
        GUI.enabled = true;
        wall.type = (Wall.WallType)EditorGUILayout.EnumPopup("Type " ,wall.type);

        if (EditorGUI.EndChangeCheck())
        {
            if (IsDestructable())
            {
                wall.gameObject.AddComponent<Health>();
            }
            else
            {
                if (wall.TryGetComponent(out Health healthScript))
                {
                    DestroyImmediate(healthScript);
                }
            }
            EditorUtility.SetDirty(target);
        }

        if (!(PrefabStageUtility.GetCurrentPrefabStage() == null))
        {
            //Checks if the wall has moved
            if(wall.transform.position != position)
            {
                position = wall.transform.position;
                
                wall.SetNewNode();
            }
        }
    }
    public bool IsDestructable()
    {
        return wall.type == Wall.WallType.Breakable;
    }
}
