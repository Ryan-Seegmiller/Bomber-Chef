using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using SerializedPropertyExtensionMethods;
using UnityEditor.Experimental.GraphView;
using System.IO;

[CustomPropertyDrawer(typeof(SearchAttribute))]
public class SearchObjectAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.width -= 60;
        EditorGUI.PropertyField(position, property, label);

        position.x += position.width;
        position.width = 60;
        if(GUI.Button(position, new GUIContent("Find")))
        {   //Gets the attribute 
            SearchAttribute attribute = property.GetPropertyAttribute<SearchAttribute>(true);
            Type t = null;
            if(attribute != null)
            {   //If the attribute is found it gets the search type from the attribute
                t = attribute._searchType;
            }
            
            string[] perDot = property.propertyPath.Split('.');
            int i = 0;
            while(t == null && i < perDot.Length)
            {   //If the attibute was not found loops through any values that if could be till it finds it or it isnt there
                t = property.serializedObject.FindProperty(perDot[i++]).GetPropertyAttribute<SearchAttribute>(true)._searchType;
            }
            if(t == null) { throw new Exception("No Type Given"); }
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), new SearchProvider(t, property));
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }

}
