using Collectible;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{

    public static ObjectPoolManager instance;

    Dictionary<string, ObjectPool<GameObject>> objectPools = new Dictionary<string, ObjectPool<GameObject>>();

    public List<ObjectPoolObjectsValues> objects = new List<ObjectPoolObjectsValues>();

    public delegate void TakeFromPool(GameObject obj);
    private TakeFromPool takeFromPool =  null;


    private int currentIndex;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SetupObjectPool();
    }
    /// <summary>
    /// Sets up the gameobjets pools
    /// </summary>
    void SetupObjectPool()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            ObjectPool<GameObject> newPool = new ObjectPool<GameObject>(() => CreatePooledItem(objects[currentIndex].obj), OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, objects[currentIndex].maxAmount, int.MaxValue);
            objectPools.Add(objects[i].name ,newPool);

        }
    }
    /// <summary>
    /// Get sthe objetcs from the assets folder
    /// </summary>
#if UNITY_EDITOR
    [ContextMenu("GetObjects")]
    public void GetObjects()
    {
        string[] GUIDS = AssetDatabase.FindAssets($"t:{nameof(ObjectsToPoolScriptableObject)}");
        objects.Clear();
        objects.TrimExcess();
        for (int i = 0; i < GUIDS.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(GUIDS[i]);
            UnityEngine.Object uncastObject = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
            ObjectsToPoolScriptableObject castObject = uncastObject as ObjectsToPoolScriptableObject;
            for (int j = 0; j < castObject.objectsForPool.Length; j++)
            {
                ObjectPoolObjectsValues poolObjectsValues = castObject.objectsForPool[j];
                objects.Add(poolObjectsValues);
            }
        }
    }
#endif
    /// <summary>
    /// Creation of object in the pool
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    GameObject CreatePooledItem(GameObject obj)
    {
        GameObject createdObj = Instantiate(obj);
        createdObj.SetActive(false);
        return createdObj;
    }
    /// <summary>
    /// Take the object from the pool
    /// </summary>
    /// <param name="obj"></param>
    private void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
        takeFromPool(obj);
        takeFromPool = null;
    }
    /// <summary>
    /// Return the ibject to the pool
    /// </summary>
    /// <param name="obj"></param>
    void OnReturnedToPool(GameObject obj)
    {
        obj.transform.parent = transform;
        obj.SetActive(false);
    }
    /// <summary>
    /// Destroy the object if theres to many in the pool
    /// </summary>
    /// <param name="obj"></param>
    void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }
    /// <summary>
    /// Gets the gameobject and sets the take from pool delegate
    /// </summary>
    /// <param name="objName"></param>
    /// <param name="takeFromPoolAction"></param>
    public void GetGameobjectFromPool(string objName, TakeFromPool takeFromPoolAction)
    {
        if (!objectPools.ContainsKey(objName)) { Debug.LogError(name + " does not contaion a refernce for " + objName); return; }

        ObjectPool<GameObject> currentPool = objectPools[objName];
        currentIndex = GetIndex(objName);
        takeFromPool += takeFromPoolAction;

       currentPool.Get();
        

    }
    /// <summary>
    /// Returns the object to the pool
    /// </summary>
    /// <param name="objName"></param>
    /// <param name="obj"></param>
    public void ReturnGameObject(string objName, GameObject obj)
    {
        if (!objectPools.ContainsKey(objName)) { 
            Debug.LogError(name + " does not contaion a refernce for " + objName); 
            return; 
        }

        ObjectPool<GameObject> currentPool = objectPools[objName];
        currentIndex = GetIndex(objName);
        if (obj.activeSelf)
        {
            currentPool.Release(obj);
        }
    }
    /// <summary>
    /// Gets the gameobject
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject GetGameobject(string name)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].name == name)
            {
                currentIndex = i;
                return objects[i].obj;
            }
        }
        return null;
    }
    /// <summary>
    /// Gets the index from the objects pool
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private int GetIndex(string name)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].name == name)
            {
                return i;
            }
        }
        return -1;
    }
}
