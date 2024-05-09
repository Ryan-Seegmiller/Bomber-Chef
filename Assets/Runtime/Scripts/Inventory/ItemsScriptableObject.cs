using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Collectible
{
    [CreateAssetMenu(fileName = "Items", menuName = "References/Items")]
    public class ItemsScriptableObject : ScriptableObject
    {
        public ItemType itemType;
        public Item[] items;
        public Dictionary<string, Item> itemsDictionary = new Dictionary<string, Item>();

        #if UNITY_EDITOR
        [ContextMenu("Generate")]
        public void GenerateItems()
        {
            itemsDictionary = new Dictionary<string, Item>();
            string[] GUIDs = AssetDatabase.FindAssets("t:texture2D", new[] { "Assets/Runtime/Resources/Textures/" + GetFolderName(itemType) });
            items = new Item[GUIDs.Length];
            for (int i = 0; i < GUIDs.Length; i++)
            {
                Item item = new Item();
                var path = AssetDatabase.GUIDToAssetPath(GUIDs[i]);
                item.sprite = Resources.Load<Sprite>(path.Split("Resources/").Last().Split(".png").First());
                item.type = itemType;
                items[i] = item;
                items[i].name = item.sprite.name;
                if (!itemsDictionary.ContainsKey(item.sprite.name))
                {
                    itemsDictionary.Add(item.sprite.name, items[i]);
                }
            }
        }
        #endif
        public void SetupDictionary()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (!itemsDictionary.ContainsKey(items[i].sprite.name))
                {
                    itemsDictionary.Add(items[i].sprite.name, items[i]);
                }
            }
        }
        public string GetFolderName(ItemType type)
        {
            switch (type)
            {
                case ItemType.Ingredient:
                    return "Ingredients";
                case ItemType.Powerup:
                    return "";
                case ItemType.Explosive:
                    return "Explosives";
                default:
                    return "";
            }
        }
    }
    [Serializable]
    public struct Item
    {
        public Sprite sprite;
        [HideInInspector] public ItemType type;
        public string name;
    }
    public enum ItemType
    {
        Ingredient,
        Powerup,
        Explosive
    }
}