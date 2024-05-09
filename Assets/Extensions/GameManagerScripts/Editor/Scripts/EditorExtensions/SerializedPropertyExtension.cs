using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SerializedPropertyExtensionMethods
{
    public static class SerializedPropertyExtension
    {
        /// <summary>
        /// Gets the attribute of the property based on the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static T GetPropertyAttribute<T>(this SerializedProperty prop, bool inherit) where T : Attribute
        {   //Returns nothing i fthere is no property
            if (prop == null) { return null; }

            //gets the type of the parent object
            Type parentObjectType = prop.serializedObject.targetObject.GetType();

            FieldInfo feildOnWhichTheAttributeIsStored = null;

            PropertyInfo propertyThatTheAttributeisStored = null;

            //loops through the path to find the correct path
            foreach (var name in prop.propertyPath.Split('.'))
            {
                feildOnWhichTheAttributeIsStored = parentObjectType.GetField(name, (BindingFlags)(-1));
                if (feildOnWhichTheAttributeIsStored == null)
                {   //if the feild is null that try the property
                    propertyThatTheAttributeisStored = parentObjectType.GetProperty(name, (BindingFlags)(-1));
                    if (propertyThatTheAttributeisStored == null)
                    {
                        return null;
                    }
                    parentObjectType = propertyThatTheAttributeisStored.PropertyType;
                }
                else
                {
                    parentObjectType = feildOnWhichTheAttributeIsStored.FieldType;
                }
            }

            T[] attributes;

            if (feildOnWhichTheAttributeIsStored != null)
            {
                attributes = feildOnWhichTheAttributeIsStored.GetCustomAttributes(typeof(T), inherit) as T[];
            }
            else if (propertyThatTheAttributeisStored != null)
            {
                attributes = propertyThatTheAttributeisStored.GetCustomAttributes(typeof(T), inherit) as T[];
            }
            else
            {
                return null;
            }
            return attributes.Length > 0 ? attributes[0] : null;
        }
        public static object GetValue(this SerializedProperty property)
        {
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fi = parentType.GetField(property.propertyPath);

            return fi.GetValue(property.serializedObject.targetObject);
        }

        public static void SetValue(this SerializedProperty property, object value)
        {
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fi = parentType.GetField(property.propertyPath);//this FieldInfo contains the type.

            fi.SetValue(property.serializedObject.targetObject, value);
        }
    }

}
