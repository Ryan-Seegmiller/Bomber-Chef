using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ParticleAttribute))]
public class ParticlePropertyDrawer : BaseStringDropdownPropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        iconPath = "d_Particle Effect";
        base.OnGUI(position, property, label);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
  

}
