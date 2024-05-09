using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReferenceAudio.AudioClips))]
public class AudioClipsEditor : PropertyDrawer
{
    //Property Values
    int propertySize = 6;
    int padding = 5;
    int propertyHeight = 18;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(position, label, property);
        Rect dropdownSize = new Rect(position.x + padding, position.y, position.width - padding, propertyHeight);
        property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(dropdownSize, property.isExpanded, property.FindPropertyRelative("name").stringValue);

        //Dropdown
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            //Creates the recs for all teh properties
            var nameRect = new Rect(position.x, position.y + propertyHeight + padding, position.width, propertyHeight);
            var clipRect = new Rect(position.x, nameRect.y + propertyHeight + padding, position.width, propertyHeight);
            var typeRect = new Rect(position.x, clipRect.y + propertyHeight + padding, position.width, propertyHeight);
            var previewButtonRect = new Rect(position.x, typeRect.y + propertyHeight + padding, position.width, propertyHeight);
            var stopPreviewButtonRect = new Rect(position.x, previewButtonRect.y + propertyHeight + padding, position.width, propertyHeight);
            //var tempIndex = new Rect(position.x, stopPreviewButtonRect.y + propertyHeight + padding, position.width, propertyHeight);
            //NOTE: in case we to fix something turn this on
            SerializedProperty type = property.FindPropertyRelative("audioClipType");

            //Creates the Property feilds
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"));
            EditorGUI.PropertyField(clipRect, property.FindPropertyRelative("clip"));
            

            
            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(typeRect, type, new GUIContent("Type"));

            if (EditorGUI.EndChangeCheck())
            {
                int enumValue = property.FindPropertyRelative("audioClipType").intValue;
                

                GetCurrentClip(property).TypeCheck((AudioManager.AudioClipType)enumValue);
            }
            //Preview Button
            if (GUI.Button(previewButtonRect, new GUIContent("Preview Sound")))
            {
                GetCurrentClip(property).Preview();//Plays the preview funcion inside of it
            }
            if (GUI.Button(stopPreviewButtonRect, new GUIContent("Stop Preview")))
            {
                GetCurrentClip(property).StopPreview();//Plays the preview funcion inside of it
            }
            //EditorGUI.PropertyField(tempIndex, property.FindPropertyRelative("index"));
            //NOTE: also this and change property size to 7

        }
       

        GetPropertyHeight(property, label);
        EditorGUI.indentLevel--;
        EditorGUI.EndFoldoutHeaderGroup();

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {   //Gets the height based of the property sizes
        return (propertyHeight + padding) * (property.isExpanded ? propertySize : 1);
    }
    private ReferenceAudio.AudioClips GetCurrentClip(SerializedProperty property)
    {
        var array = property.serializedObject.targetObject.GetType().GetField("clips").GetValue(property.serializedObject.targetObject);//Gets the object in which the array is being stored in then gets the array
        int index = property.FindPropertyRelative("index").intValue;
        var clip = ((ReferenceAudio.AudioClips[])array)[index];//Gets the clip Struct
        return clip;
    }
}
