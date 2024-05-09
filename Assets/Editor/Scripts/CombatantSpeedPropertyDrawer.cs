using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CombatantSpeed))]
public class CombatantSpeedPropertyDrawer : PropertyDrawer
{
    private readonly string[] popupOptions = { "use local", "use global" };

    public GUIStyle popupStyle;


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(popupStyle == null)
        {
            popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
            popupStyle.imagePosition = ImagePosition.ImageOnly;
        }

        label = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        EditorGUI.BeginChangeCheck();

        SerializedProperty useLocal = property.FindPropertyRelative("localSpeedToggle");
        SerializedProperty globalValue = property.FindPropertyRelative("globalSpeed");
        SerializedProperty localValue = property.FindPropertyRelative("localSpeed");

        Rect buttonRect = new Rect(position);
        buttonRect.yMin += popupStyle.margin.top;
        buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
        position.xMin = buttonRect.xMax;

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        int result = EditorGUI.Popup(buttonRect, useLocal.boolValue ? 0:1, popupOptions, popupStyle);

        useLocal.boolValue = result == 0;

        EditorGUI.PropertyField(position, useLocal.boolValue ? localValue : globalValue,GUIContent.none);

        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();

    }
}
