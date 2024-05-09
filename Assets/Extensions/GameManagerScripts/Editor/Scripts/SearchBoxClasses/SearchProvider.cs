using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class SearchProvider : ScriptableObject, ISearchWindowProvider
{
    Type assetType = null;
    public SerializedProperty serializedProperty;

    object[] _objectList = null;
    List<string> _AllStringsList = new List<string>();
    public SearchProvider(Type assetType, SerializedProperty serializedProperty)
    {
        this.assetType = assetType;
        this.serializedProperty = serializedProperty;

        GetStringsFromAssets();
    }
    public SearchProvider(object[] _objectList, SerializedProperty serializedProperty)
    {
        this._objectList = _objectList;
        this.serializedProperty = serializedProperty;

        GetStrings();
    }
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchList = AddToSearchTree();

        return searchList;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
       if(assetType != null) 
        { 
            serializedProperty.objectReferenceValue = (UnityEngine.Object)SearchTreeEntry.userData;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }
       else if(_objectList != null)
        {
            if(serializedProperty.type == "string")
            {
               serializedProperty.stringValue = (string)SearchTreeEntry.userData;
               serializedProperty.serializedObject.ApplyModifiedProperties();
            }
            
        }
        

        
       
        return true;
    }
    string[] _stringList = null;
    public void GetStrings()
    {
        _stringList = new string[_objectList.Length];
        for (int i = 0; i < _objectList.Length; i++)
        {
            _stringList[i] = _objectList[i].ToString();
            Type type = _objectList[i].GetType();//Gets the type of the object

            FieldInfo[] fields = type.GetFields();//Gets the feilds using the type of the object passed in
            int j = 0;
            foreach (FieldInfo field in fields)
            {
                Array fieldArray = field.GetValue(_objectList[i]) as Array;//Sets the array of structs insised the scriptable object to this array
                string[] strings = new string[fieldArray.Length];//Sets the length of the strings

                foreach (var item in fieldArray)
                {
                    FieldInfo[] structFields = item.GetType().GetFields();//Gets the fields withon the given struct

                    foreach (var finalItem in structFields)
                    {
                        if (finalItem.GetValue(item).GetType() == typeof(string))//Checks if otf a string then adds it to the strings array
                        {
                            strings[j] = (string)finalItem.GetValue(item);
                        }
                    }
                    j++;
                }
                foreach (string str in strings)
                {
                    string subString = _objectList[i] + "/" + str;
                    _AllStringsList.Add(subString);
                }
                break;
            }
        }
    }
    public void GetStringsFromAssets()
    {
        string[] assetGuids = AssetDatabase.FindAssets($"t:{assetType.Name}");
        List<string> paths = new List<string>();
        foreach (string assetGuid in assetGuids)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(assetGuid));
        }
        _AllStringsList = paths;
    }

   
    public List<SearchTreeEntry> AddToSearchTree()
    {
        List<SearchTreeEntry> searchList = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Search Menu"), 0)
        };

        List<string> groups = new List<string>();
        foreach (string item in _AllStringsList)
        {
            string[] entryTitle = item.Split('/');
            string groupName = "";
            for (int i = 0; i < entryTitle.Length - 1; i++)
            {
                groupName = entryTitle[i];
                if (!groups.Contains(groupName))
                {
                    searchList.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                    groups.Add(groupName);
                }
                groupName += "/";
            }
            SearchTreeEntry entry = null;
            if (assetType != null)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item);
                entry = new SearchTreeEntry(new GUIContent(entryTitle.Last(), EditorGUIUtility.ObjectContent(obj, obj.GetType()).image));
                entry.level = entryTitle.Length;
                entry.userData = obj;
            }
            else if (_objectList != null)
            {
                entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
                entry.level = entryTitle.Length;
                entry.userData = entryTitle.Last();
            }

            searchList.Add(entry);
        }

        return searchList;
    }
}
