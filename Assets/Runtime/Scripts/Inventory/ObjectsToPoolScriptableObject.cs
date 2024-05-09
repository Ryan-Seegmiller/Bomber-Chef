using Collectible;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectPool", menuName = "Object pool")]
public class ObjectsToPoolScriptableObject : ScriptableObject
{
    public ObjectPoolObjectsValues[] objectsForPool;
    private void OnValidate()
    {
        for(int i = 0;i < objectsForPool.Length; i++)
        {
            if (objectsForPool[i].obj.TryGetComponent(out ItemHelper itemHelper))
            {
                objectsForPool[i].name = itemHelper.itemName;
            }
        }
    }
}
[Serializable]
public struct ObjectPoolObjectsValues
{
    public string name;
    public GameObject obj;
    public int maxAmount;
}
