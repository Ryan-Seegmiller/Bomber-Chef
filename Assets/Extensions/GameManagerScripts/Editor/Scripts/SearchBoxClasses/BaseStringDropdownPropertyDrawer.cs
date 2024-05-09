using SerializedPropertyExtensionMethods;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BaseStringDropdownPropertyDrawer : PropertyDrawer
{
    //Indexs
    protected int currentIndex;
    protected int referenceIndex;
    //Object array
    protected object[] objs;
    //String arrays
    protected string[] labels;
    protected string[] referencesArray;
    //Icon Data
    protected string iconPath;

    //Positional Data
    int propertyHeight = 40;
    int dropdownHeight = 16;
    int dropdownSpacing = 10;
    int dropdownWidth;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        try
        {
            //Gets the object list
            objs = property.GetPropertyAttribute<BaseGameManagerAttribute>(true).objectArray;
        }
        catch
        {
            if (objs == null)
            {   //If the attribute is found it gets the search type from the attribute
                string[] perDot = property.propertyPath.Split('.');
                int i = 0;
                while (objs == null && i < perDot.Length)
                {   //If the attibute was not found loops through any values that if could be till it finds it or it isnt there
                    objs = property.serializedObject.FindProperty(perDot[i++]).GetPropertyAttribute<BaseGameManagerAttribute>(true).objectArray;
                }
            }
        }

        

        //Gets the refernece array and current index for the refernece array
        referencesArray = GetRefernceNames(objs);
        referenceIndex = GetCurrentReferenceIndex(property, objs).GetValueOrDefault();

        //Sitches the refernces array
        SwitchReferences();

        //Gets the the index
        currentIndex = GetCurrentIndex(property, labels).GetValueOrDefault();

        #region Positions
        //Positions

        dropdownWidth = (int)position.width / 3;

        Rect refernecesArrayPopupBounds = new Rect( dropdownSpacing*10 - position.x, position.y + position.height / 2, dropdownWidth, dropdownHeight);
        Rect popupPosition = new Rect(dropdownSpacing*10 - position.x + dropdownWidth, position.y + position.height / 2, dropdownWidth, dropdownHeight);
        Rect buttonRect = new Rect(  dropdownSpacing+ popupPosition.x + popupPosition.width, position.y + position.height/2, 40, dropdownHeight);
        #endregion

        #region Icon and alignment
        GUIStyle labelStyle = GUI.skin.GetStyle("label");
        labelStyle.alignment = TextAnchor.UpperCenter;

        //Adds an icon
        GUIContent icon = EditorGUIUtility.IconContent(iconPath);
        GUIStyle iconStyle = new GUIStyle();
        iconStyle.alignment = TextAnchor.UpperRight;
        Vector2 labelSize = labelStyle.CalcSize(new GUIContent(label.text));
        iconStyle.padding.right = (int)(labelSize.x/2);
              

        var iconPos = position;
        iconPos.height = position.height/2;
        iconPos.width = position.width/2;
        #endregion  


        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.LabelField(position, label,labelStyle);//label for the property
        EditorGUI.LabelField(iconPos, icon, iconStyle);
       
        //Draws the refernece popup
        DrawPopup(refernecesArrayPopupBounds, ref referenceIndex, referencesArray, property, true);

        
        //Value popup that takes in the correct value for the object to refernce
        DrawPopup(popupPosition, ref currentIndex, labels, property, false);

       
        if (GUI.Button(buttonRect, new GUIContent("Find")))
        {
            if (objs != null)
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), new SearchProvider(objs, property));
            }

        }
        //Makes sure it sets the sound if no sound is set

        if (property.stringValue == "")
        {
            property.stringValue = "";
        }

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return propertyHeight;
    }
    /// <summary>
    /// Draws the popup for the differnt values based on the indexs
    /// </summary>
    /// <param name="popupPosition"></param>
    /// <param name="index"></param>
    /// <param name="displayStrings"></param>
    /// <param name="property"></param>
    /// <param name="isRefernece"></param>
    public void DrawPopup(Rect popupPosition, ref int index, string[] displayStrings, SerializedProperty property, bool isRefernece)
    {
        EditorGUI.BeginChangeCheck();
        index = EditorGUI.Popup(popupPosition, index, displayStrings);//Draws the popup

        bool changed = EditorGUI.EndChangeCheck();

        if (changed)
        {
            if(!isRefernece)
            {   //If its not a refernce than set the property string value equal to the string at the current index of the popup
                property.stringValue = displayStrings[currentIndex];
            }
            else
            {   //If it is a refernce and has been changed set the string equal to the first in the refernce that has been changed to
                SwitchReferences();
                property.stringValue = labels[0];
            }
        }
        
    }
    /// <summary>
    /// Gets the current index witin the array
    /// </summary>
    /// <param name="property"></param>
    /// <param name="labels"></param>
    /// <returns></returns>
    public int? GetCurrentIndex(SerializedProperty property, string[] labels)
    {
        for(int i = 0; i < labels.Length; i++)
        {
            //Loops through the label to determine where the string belongs
            if (property.stringValue == labels[i])
            {
                return i;
            }
        }
        return null;
    }
    /// <summary>
    /// Gets the current index of the refenrce for the sound
    /// </summary>
    /// <typeparam name="t"></typeparam>
    /// <param name="property"></param>
    /// <param name="array"></param>
    /// <returns></returns>
    public int? GetCurrentReferenceIndex<t>(SerializedProperty property, t[] array)
    {
        for (int i = 0; i < referencesArray.Length; i++)
        {   //Loops through each rfenerce to find which refernce the string belongs to
            var stringArr = GetStrings(array[i]);
            int? checkint = GetCurrentIndex(property, stringArr);
            if(checkint == null)
            {
                continue;
            }
            else
            {
                return i;
            }
        }
        return null;
    }
    /// <summary>
    ///switching the reference
    /// </summary>
    public void SwitchReferences()
    {
        try
        {
            labels = GetStrings(objs[referenceIndex]);
        }
        catch (Exception)
        {
            labels = referencesArray;
            Debug.LogWarning($"There is no {objs.GetType()} in Game Manager parent class");
        }
        
    }

    /// <summary>
    /// Gets all the strings needed to use as labels inside the selectors
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string[] GetStrings(object obj)
    {
        string[] strings = null;

        Type type = obj.GetType();//Gets the type of the object

        FieldInfo[] fields = type.GetFields();//Gets the feilds using the type of the object passed in
        int i = 0;
        foreach (FieldInfo field in fields)
        {
            Array fieldArray = field.GetValue(obj) as Array;//Sets the array of structs insised the scriptable object to this array
            strings = new string[fieldArray.Length];//Sets the length of the strings

            foreach (var item in fieldArray)
            {
                FieldInfo[] structFields = item.GetType().GetFields();//Gets the fields withon the given struct

                foreach (var finalItem in structFields)
                {
                    if (finalItem.GetValue(item).GetType() == typeof(string))//Checks if otf a string then adds it to the strings array
                    {
                        strings[i] = (string)finalItem.GetValue(item);
                    }
                }
                i++;
            }
            break;
        }


        return strings;
    }
    /// <summary>
    /// Gets the names in the arrays of refenerces
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="referneceArray"></param>
    /// <returns></returns>
    public static string[] GetRefernceNames<T>(T[] referneceArray)
    {
        if(referneceArray == null) {return new string[] {""};}
        //Initailises string array
        string[] refNames = new string[referneceArray.Length];
        int i = 0;
        foreach (var _ref in referneceArray)
        {
            //Sets the name of the refernece inside the array
            var props = _ref.GetType().GetProperties();
            foreach(var prop in props)
            {
                if(prop.PropertyType == typeof(string))
                {
                    refNames[i++] = (string)prop.GetValue(_ref);
                }
            }
        }

        return refNames;
    }
}
