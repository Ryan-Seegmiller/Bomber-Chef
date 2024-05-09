using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldHearts : MonoBehaviour
{
    public TextMeshProUGUI worldNameText;
    public RectTransform worldHeartHolder;
    [HideInInspector]public RectTransform rectTR;
    private Image[] worldHearts;

    public Stack<Image> worldHeartsStack;

    public void SetName(string name)
    {
        BaseHUD.ChangeName(name, worldNameText);
    }
    public void SetupWorldLives(int liveCount, GameObject heart)
    {
        //Sets up the rectTR for use by the combatants
        rectTR = GetComponent<RectTransform>();

        //Array and other setup
        worldHearts = new Image[liveCount];
        worldHeartsStack = new Stack<Image>(liveCount);
        worldHeartHolder.localScale = Vector3.one / 2;

        for (int i = 0; i < worldHearts.Length; i++)
        {
            //creates a heart for the world
            GameObject worldHeart = Instantiate(heart, worldHeartHolder);

            worldHearts[i] = worldHeart.GetComponentInChildren<Image>();

            worldHeartsStack.Push(worldHearts[i]);
        }
    }
    public void SetTextColor(Color color)
    {
        GetComponentInChildren<TextMeshProUGUI>().color = color;
    }
    public void RemoveHearts(int damage)
    {
        for (int i = 0; i < damage; i++)
        {
            Image worldHeart = worldHeartsStack.Pop();
            worldHeart.gameObject.SetActive(false);
        }
    }
    public void Death()
    {
        worldNameText.gameObject.SetActive(false);
    }

}
