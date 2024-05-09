using UnityEngine;
using UnityEditor;
using Codice.CM.Client.Differences;
using System.Reflection.Emit;

[CustomPropertyDrawer(typeof(ObjectPoolAttribute))]
public class ObjectPoolPropertyDrawer : PropertyDrawer
{
    #region Declerations
    string[] objectLabels = null;
    string[] objectNames = null;
    string label;
    #endregion

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Start methods
        if(objectNames == null || objectLabels == null)
        {
            GetObjects();
        }
        
        int currentIndex = GetIndex(property);
        
        EditorGUI.BeginChangeCheck();
        //Positions
        Rect popupPosition = new Rect(position.x + position.width/2, position.y, position.width/2, position.height);
        Rect popupLabel = new Rect(position.x, position.y, position.width, position.height);

        //Labels
        EditorGUI.LabelField(popupLabel, label);

        currentIndex = EditorGUI.Popup(popupPosition, currentIndex, objectLabels);

        if(EditorGUI.EndChangeCheck())
        {
            property.stringValue = objectNames[currentIndex];
        }

    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
    /// <summary>
    /// Sets the strings
    /// </summary>
    public void GetObjects()
    {
        string[] GUIDS = AssetDatabase.FindAssets($"t:{nameof(ObjectsToPoolScriptableObject)}");
        int arraylength = 0;
        foreach( string GUID in GUIDS )
        {
            string path = AssetDatabase.GUIDToAssetPath(GUID);
            Object uncastObject = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
            ObjectsToPoolScriptableObject castObject = uncastObject as ObjectsToPoolScriptableObject;
            arraylength += castObject.objectsForPool.Length;
        }
        objectNames = new string[arraylength];
        objectLabels = new string[arraylength];

        int namesIndex = 0;
        for (int i = 0; i < GUIDS.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(GUIDS[i]);
            Object uncastObject = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
            ObjectsToPoolScriptableObject castObject = uncastObject as ObjectsToPoolScriptableObject;
            for (int j = 0; j < castObject.objectsForPool.Length; j++)
            {
                ObjectPoolObjectsValues poolObjectsValues = castObject.objectsForPool[j];
                objectNames[namesIndex] = poolObjectsValues.name;
                objectLabels[namesIndex] = castObject.name + " : " + poolObjectsValues.name;
                namesIndex++;
            }
        }
    }
    /// <summary>
    /// Gets the index
    /// </summary>
    /// <param name="property"></param>
    public int GetIndex(SerializedProperty property)
    {
        int currentIndex = 0;
        for (int i = 0; i < objectNames.Length; i++)
        {
            if(property.stringValue == objectNames[i])
            {
                currentIndex = i;
                return currentIndex;
            }
        }
        
        property.stringValue = objectNames[currentIndex];
        return currentIndex;
    }
}
