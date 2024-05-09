using UnityEngine;
using UnityEditor;
using System;

public static class AudioManagerCreator
{
    #if UNITY_EDITOR
    [MenuItem("GameObject/Audio/AudioManager")]
    static void CreateAudioManager()
    {
        //Checks if there is a audio manager already in the scene
        AudioManager current = AudioManager.GetInstance();
        if (current != null) 
        {
            //Does a popup if there is already the gameobject
            bool findObject = EditorUtility.DisplayDialog("STOP", "An AudioManager already exsists in the scene", "Find", "Replace" );
            if (findObject)
            {
                Selection.activeGameObject = current.gameObject; 
                return;
            }
            else
            {
                //Destroys the current AudioManager
                UnityEngine.Object.DestroyImmediate(current.gameObject);
            }
           
        }
        //Sets the name of tyhe new gameobject
        GameObject gameObject = new GameObject("Auido Manager"); 
        
        //Sets the selection active
        if (Selection.activeObject)
        {
            gameObject.transform.SetParent(Selection.activeGameObject.transform);
            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        //Makes sure its selected
        Selection.activeGameObject = gameObject;

        //Register to undo creation
        Undo.RegisterCreatedObjectUndo(gameObject, "Created Audio Manager");

        //Creates the audio Manager
        AudioManager audioManager = gameObject.AddComponent<AudioManager>();
        audioManager.GenerateAudioManager();

        EditorApplication.delayCall += () =>
        {
            // rename
            Type sceneHierarchyType = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor");
            EditorWindow hierarchWindow = EditorWindow.GetWindow(sceneHierarchyType);
            hierarchWindow.SendEvent(EditorGUIUtility.CommandEvent("Rename"));
        };
    }
    #endif
}
