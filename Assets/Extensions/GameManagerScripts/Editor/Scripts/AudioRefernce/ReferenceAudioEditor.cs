using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReferenceAudio))]
public class ReferenceAudioEditor : Editor
{
    ReferenceAudio m_ReferenceAudio;
    int currentIndex;
    public override void OnInspectorGUI()
    {
        m_ReferenceAudio = target as ReferenceAudio;
        //base.OnInspectorGUI();
        serializedObject.Update();

        SerializedProperty clipsArray = serializedObject.FindProperty("clips");

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(clipsArray, true);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            m_ReferenceAudio.clips[m_ReferenceAudio.clips.Length - 1].index = m_ReferenceAudio.clips.Length - 1;//Sets the index to the appropiate index
           
        }
    }
}
