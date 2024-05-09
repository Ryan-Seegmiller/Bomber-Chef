using UnityEditor;
using UnityEngine;
using Collectible;

[CustomEditor(typeof(ItemHelper))]
public class ItemHelperCustomEditor : Editor
{
    ItemHelper m_itemHelper;
    int? curentIndex;
    string[] itemsForDisplay;

    public override void OnInspectorGUI()
    {

        m_itemHelper = (ItemHelper)target;//Sets the target

        if(m_itemHelper.gameObject.layer != 13)
        {
            m_itemHelper.gameObject.layer = 13;
        }
        GetIndex();//Gets the current index
        #region Type Set
        EditorGUI.BeginChangeCheck();

        m_itemHelper.type = (ItemType)EditorGUILayout.EnumPopup("Type of Item: ", m_itemHelper.type);  

        if (EditorGUI.EndChangeCheck())
        {
            FindNames();
            SetString();
            EditorUtility.SetDirty(target);
        }
        #endregion

        #region String Set
        EditorGUI.BeginChangeCheck();

        curentIndex = EditorGUILayout.Popup("Item name: ", (int)curentIndex, itemsForDisplay);

        if (EditorGUI.EndChangeCheck())
        {
            SetString();
            EditorUtility.SetDirty(target);
        }
        #endregion

        SerializedProperty durationProperty = serializedObject.FindProperty("timeTillDissapear");
        EditorGUILayout.PropertyField(durationProperty);

        SerializedProperty eventProperty = serializedObject.FindProperty("onPickUp");
        EditorGUILayout.PropertyField(eventProperty);
        serializedObject.ApplyModifiedProperties();
    }
    /// <summary>
    /// Finds the strings
    /// </summary>
    public void FindNames()
    {   //Gets the GUIDs of the items
        string[] GUIDS = AssetDatabase.FindAssets($"t:{nameof(ItemsScriptableObject)}");
        for(int i = 0; i < GUIDS.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(GUIDS[i]);
            Object uncastObject = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
            ItemsScriptableObject castObject = uncastObject as ItemsScriptableObject;
            if(castObject.itemType == m_itemHelper.type)
            {
                int itemAmount = castObject.items.Length;
                itemsForDisplay = new string[itemAmount];
                for (int j = 0; j < itemAmount; j++)
                {
                    itemsForDisplay[j] = castObject.items[j].name;//Sets the string names for the popup
                }
            }
        }
        curentIndex = 0;
       
    }
    /// <summary>
    /// Sets the string value
    /// </summary>
    public void SetString()
    {
        m_itemHelper.itemName = itemsForDisplay[(int)curentIndex];
        m_itemHelper.name = itemsForDisplay[(int)curentIndex];
    }
    /// <summary>
    /// Gets the index of the current object if there is none
    /// </summary>
    public void GetIndex()
    {
        if(curentIndex != null) { return; }
        FindNames();
        for (int i = 0; i < itemsForDisplay.Length; i++)
        {
            if(m_itemHelper.itemName == itemsForDisplay[i])
            {
                curentIndex = i;
                break;
            } 
        }
        SetString();
    }
}


