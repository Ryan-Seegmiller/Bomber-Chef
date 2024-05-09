using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]

public abstract class BaseGameManagerAttribute : PropertyAttribute
{
    public abstract object[] objectArray { get; }
}
