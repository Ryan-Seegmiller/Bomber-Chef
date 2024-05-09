using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class BaseHUD : MonoBehaviour
{
    public Image[] images = new Image[3];

    public InventoryManager inventoryManager;

    public void AddSprite(Sprite sprite)
    {
        if (inventoryManager.itemNames.Count >= inventoryManager.invertoryCapacity) { return; }

        Sprite[] sprites = GetSpriteArray();

        for (int i = 1; i < sprites.Length; i++)
        {
            images[i].sprite = sprites[i - 1];
            if (images[i].sprite == null) { break; }
            images[i].color = Color.white;
        }
        images[0].sprite = sprite;
        images[0].color = Color.white;
       
    }
    public void RemoveSprite()
    {
        Sprite[] sprites = GetSpriteArray();

        images[0].sprite = null;

        for (int i = 0; i < sprites.Length; i++)
        {
            //Makes sure there is no index out of range exception
            if(i + 1 >= inventoryManager.invertoryCapacity)
            {
                images[i].sprite = null;
                images[i].color = Color.clear;
                break;
            }
            images[i].sprite = sprites[i + 1];
            //Sets the image to transparent and breaks if at the end
            if (images[i].sprite == null) 
            { 
                images[i].color = Color.clear; 
                break; 
            }

        }
    }
    private Sprite[] GetSpriteArray()
    {
        Sprite[] sprites = new Sprite[inventoryManager.invertoryCapacity];
        for (int i = 0; i < inventoryManager.invertoryCapacity; i++)
        {
            sprites[i] = images[i].sprite;
        }
        return sprites;
    }

    public void RemoveSprites()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].sprite = null;
            images[i].color = Color.clear;

        }
    }

    public static void ChangeName(string text, TextMeshProUGUI nameText)
    {
        nameText.text = text;
    }
}
