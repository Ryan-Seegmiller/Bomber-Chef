using Collectible;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Recipe")]
public class RecipesScriptableObject : ScriptableObject
{
    public int ingredientAmount;
    [SerializeField, HideInInspector]private int ingredientAmountBefore;
    public string ricketyBomb;
    public Recipe[] recipes = null;

    public string CheckRecipe(Stack<string> items)
    {
        string[] itemsArray = items.ToArray();
        foreach (Recipe recipe in recipes)
        {
            bool recipeFound = true;
            //Sets an empty array of ints to make sure ther eare no duplicates being checked
            int[] ingreidentsFound = new int[recipe.ingredients.Length];
            for (int j = 0; j < ingreidentsFound.Length; j++)
            {
                ingreidentsFound[j] = -1;
            }
            for (int i = 0; i < itemsArray.Length; i++) 
            {
               
                //Checks if the ingredient is in the recipe
                for (int j = 0; j < recipe.ingredients.Length; j++)
                {
                    if (recipe.ingredients[j] == itemsArray[i])
                    {   //Checks if the ingredient at that index has already been inserted
                        if (!ingreidentsFound.Contains(j))
                        {
                            ingreidentsFound[i] = j;
                            break;
                        }
                    }
                }
                //If an ingerideint was not found then break
                if (ingreidentsFound[i]  == -1)
                {
                    recipeFound = false;
                    break;
                }
            }
            if (recipeFound)
            {
                return recipe.cookedItem;
            }
        }

        return ricketyBomb;

    }
    public bool CheckForItem(Recipe recipe, ItemHelper item)
    {
        if(item == null)
        {
            return false;
        }
        return recipe.ingredients.Contains(item.name);
    }
    /// <summary>
    /// Checks the recipe against the current inventory to see if there is need for an item
    /// </summary>
    /// <param name="recipe"></param>
    /// <param name="item"></param>
    /// <param name="inventory"></param>
    /// <returns></returns>
    public bool CheckIfItemNeeded(Recipe recipe, ItemHelper item, Stack<string> inventory)
    {
        if (item == null){ return false; }//If the item is null

        int amountOfItemsInRecipe = 0;//intializes the amount of items in recipe
        //Searches for the item in recipe
        foreach (string recipeItem in recipe.ingredients) 
        { 
            if(recipeItem == item.itemName)
            {
                amountOfItemsInRecipe++;
            }
        }
        //returns false if there is no item in the recipe
        if(amountOfItemsInRecipe == 0)
        {
            return false;
        }
        //Searches for the item in inventory
        int amountOfItemInInvenotry = 0;
        string[] currentInventory = inventory.ToArray();//Turns inventory into array for ease of lookup
        foreach (string itemInInventory in currentInventory) 
        {
            if(itemInInventory == item.itemName)
            {
                amountOfItemInInvenotry++;
            }
        }

        return amountOfItemInInvenotry < amountOfItemsInRecipe;
    }

    private void OnValidate()
    {
        if (ingredientAmount != ingredientAmountBefore)
        {
           
            for (int i = 0; i < recipes.Length; i++)
            {
                string[] tempRecipe = recipes[i].ingredients;
                recipes[i].ingredients = new string[ingredientAmount];
                for(int j = 0; j < recipes[i].ingredients.Length; j++)
                {
                    recipes[i].ingredients[j] = tempRecipe[j];
                }
            }
            ingredientAmountBefore = ingredientAmount;
        }
    }
}

