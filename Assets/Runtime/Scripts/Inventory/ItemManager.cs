using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Collectible
{ 
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager instance { get; private set; }
        public Dictionary<ItemType, ItemsScriptableObject> itemRefernces;
        public ItemType[] types;
        public ItemsScriptableObject[] itemsScriptableObjects;
        [HideInInspector]public List<ItemHelper> items = new List<ItemHelper>();


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public void Start()
        {
            PopulateDictionary();
        }

        public Sprite GetItemSprite(ItemType type, string name)
        {
            return itemRefernces[type].itemsDictionary[name].sprite;
        }
        #if UNITY_EDITOR
        [ContextMenu("Populate Item Refernces")]
        public void PopulateItemRefernces()
        {
            if (itemRefernces != null) { return; }

            string[] GUIDS = AssetDatabase.FindAssets($"t:{nameof(ItemsScriptableObject)}");
            types = new ItemType[GUIDS.Length];
            itemsScriptableObjects = new ItemsScriptableObject[GUIDS.Length];
            for (int i = 0; i < GUIDS.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(GUIDS[i]);
                Object uncastObject = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
                ItemsScriptableObject castObject = uncastObject as ItemsScriptableObject;
                types[i] = castObject.itemType;
                itemsScriptableObjects[i] = castObject;
                
                
            }
        }
        #endif
        void PopulateDictionary()
        {
            itemRefernces = new Dictionary<ItemType, ItemsScriptableObject>();

            for (int i = 0; i < types.Length; i++)
            {
                itemRefernces.Add(types[i], itemsScriptableObjects[i]);
                itemsScriptableObjects[i].SetupDictionary();
            }
        }
        public void AddItem(ItemHelper item)
        {
            items.Add(item);
        }
        public void RemoveItem(ItemHelper item)
        {
            items.Remove(item);
        }
        public void ClearItems(ItemHelper item)
        {
            items.Clear();
        }
        
    }
}
