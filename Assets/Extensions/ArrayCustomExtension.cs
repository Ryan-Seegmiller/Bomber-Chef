using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace System.Collections
{
    public static class ArrayCustomExtension
    {
        public static T[] GetSection<T>(this T[] array, int begining, int end)
        {
            T[] newArray = new T[end - begining];
            for(int i = begining; i < end; i++)
            {
                newArray[i] = array[i];
            }
            return newArray;
        }
        public static T[] GetSection<T>(this T[] array, int end)
        {
            T[] newArray = new T[end];
            for(int i = 0; i < end; i++)
            {
                newArray[i] = array[i];
            }
            return newArray;
        }
    }
}
