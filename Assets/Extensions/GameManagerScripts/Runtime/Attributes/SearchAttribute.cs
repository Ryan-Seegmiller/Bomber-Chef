using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.All,AllowMultiple = true, Inherited = true)]
public class SearchAttribute : PropertyAttribute
{
    public Type _searchType;
    public SearchAttribute(Type _searchType) 
    {
        this._searchType = _searchType;
    }
   
   
}
