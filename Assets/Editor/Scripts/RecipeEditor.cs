using Collectible;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomPropertyDrawer(typeof(Recipe))]
public class RecipeEditor : PropertyDrawer
{
    SerializedProperty recipeName;
    SerializedProperty objectField;
    SerializedProperty ingriedientsArray;
    int objectIndex;

    float propertyHeight = 20;
    float propertyCount = 4;
    float topMargin = 5;
    float arraySize = 0;

    bool foldout;
    bool arrayFoldout;

    List<string> objectNames = new List<string>();
    List<string> ingredientNames = new List<string>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

       if(objectNames.Count <= 0)
        {
            GetObjects();
        }
        if (ingredientNames.Count <= 0)
        {
            GetIngredients();
        }


        Rect foldoutRect = new Rect(position.x, (foldout)? position.y - propertyHeight/propertyCount + 1 : position.y + position.height/propertyCount, position.width, position.height/propertyCount + 1);


        foldout = EditorGUI.Foldout(foldoutRect, foldout, GUIContent.none);
        if (foldout)
        {

            Rect nameRect = new Rect(position.x, position.y + propertyHeight, position.width, propertyHeight);
            recipeName = property.FindPropertyRelative("name");
            recipeName.stringValue = EditorGUI.TextField(nameRect, recipeName.displayName, recipeName.stringValue);

            EditorGUI.BeginChangeCheck();

            Rect objectLabelRect = new Rect(position.x, position.y + propertyHeight* 2 + topMargin, position.width/3, propertyHeight);
            Rect objectRect = new Rect(position.x + position.width/3, position.y + propertyHeight * 2 + topMargin, position.width/2, propertyHeight);

            objectField = property.FindPropertyRelative("cookedItem");
            EditorGUI.LabelField(objectLabelRect, objectField.displayName);
            objectIndex = EditorGUI.Popup(objectRect, objectIndex, objectNames.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                objectField.stringValue = objectNames[objectIndex];
            }

            Rect arrayFoloutRect = new Rect(position.x, position.y + propertyHeight * 3 + topMargin * 2, position.width, propertyHeight + topMargin);

            arrayFoldout = EditorGUI.Foldout(arrayFoloutRect, arrayFoldout, GUIContent.none);
            
            if (arrayFoldout)
            {
                ingriedientsArray = property.FindPropertyRelative("ingredients");
                arraySize = ingriedientsArray.arraySize;
                Rect arrayPos = new Rect(position.x, position.y + propertyHeight * 3 + topMargin * 3, position.width, propertyHeight);
                 for (int i = 0; i < ingriedientsArray.arraySize; i++)
                 {
                    arrayPos.y += propertyHeight + topMargin;
                    ingriedientsArray.GetArrayElementAtIndex(i).stringValue = ingredientNames[EditorGUI.Popup(arrayPos,GetIndex(ingriedientsArray.GetArrayElementAtIndex(i).stringValue), ingredientNames.ToArray())];
                 }  
            }
            else
            {
                arraySize = 0;
            }
        }


        
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = (foldout) ? base.GetPropertyHeight(property, label) + propertyHeight * (propertyCount + arraySize + 1): base.GetPropertyHeight(property, label);
        return height;
        ;
    }
    private void GetObjects()
    {
        string[] GUIDS = AssetDatabase.FindAssets($"t:{nameof(ObjectsToPoolScriptableObject)}");
        objectNames.Clear();
        for (int i = 0; i < GUIDS.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(GUIDS[i]);
            UnityEngine.Object uncastObject = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
            ObjectsToPoolScriptableObject castObject = uncastObject as ObjectsToPoolScriptableObject;
            for (int j = 0; j < castObject.objectsForPool.Length; j++)
            {
                ObjectPoolObjectsValues poolObjectsValues = castObject.objectsForPool[j];
                objectNames.Add(poolObjectsValues.name);
            }
        }
    }
    private void GetIngredients()
    {
        string[] GUIDS = AssetDatabase.FindAssets($"t:{nameof(ItemsScriptableObject)}");
        for (int i = 0; i < GUIDS.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(GUIDS[i]);
            Object uncastObject = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
            ItemsScriptableObject castObject = uncastObject as ItemsScriptableObject;
            if (castObject.itemType != ItemType.Ingredient) { continue; }
            foreach(Item item in castObject.items)
            {
                ingredientNames.Add(item.name);
            }
        }
    }
    private int GetIndex(string name)
    {
        for (int i = 0; i < ingredientNames.Count; i++)
        {
            if (ingredientNames[i] == name)
                return i;
        }
        return 0;
    }
}
