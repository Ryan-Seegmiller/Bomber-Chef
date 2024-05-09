using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AudioAttribute))]
public class AudioPropertyDrawer : BaseStringDropdownPropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        iconPath = "AudioClip Icon";
        base.OnGUI(position, property, label);
       
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }

}
