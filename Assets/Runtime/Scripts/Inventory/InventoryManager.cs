using Collectible;
using System;
using System.Collections.Generic;

[Serializable]
public class InventoryManager
{
    //Stack
    public int invertoryCapacity = 3;
    public Stack<string> itemNames;

    //Delegates
    public delegate void AddIngredient(ItemHelper item);
    public delegate void SubtractIngredient();

    public AddIngredient addIngredient;
    public SubtractIngredient subtractIngredient;

    private BaseHUD hud = null;

    // Start is called before the first frame update
    public InventoryManager()
    {
        itemNames = new Stack<string>(invertoryCapacity);
        AddDelagate();
    }
    public InventoryManager(BaseHUD hud)
    {
        this.hud = hud;
        itemNames = new Stack<string>(invertoryCapacity);
        subtractIngredient += hud.RemoveSprite;
        hud.inventoryManager = this;
        AddDelagate();

    }

    void AddItem(ItemHelper item)
    {
        if(hud != null)
        {
            hud.AddSprite(item.GetSprite());
        }
        
        itemNames.Push(item.itemName);
       
    }
    void SubtractItem()
    {
        itemNames.Pop();
    }
    public void AddDelagate()
    {
        addIngredient += AddItem;
        subtractIngredient += SubtractItem;
    }

}
