using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [Header("Item Interact")]
    public TextMeshProUGUI itemIneract;
    public TextMeshProUGUI inventoryFullText;
    [SerializeField] private string baseInteractText;

    #region Referneces
    public TextMeshProUGUI gameTimer;
    public GameObject leaderBoard;
    public GameObject BackToMenuButton;
    public GameObject continueButton;
    public GameObject PlayerDeathUI;//The player death UI
    public PlayerHud playerHud;
    public Transform playerHudHolder;
    #endregion

    #region Setup
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
    private void Start()
    {
        itemIneract.enabled = false;
    }
    #endregion

    #region ItemInetract
    public void SetItemInteract(string name)
    {
        itemIneract.enabled = true;
        itemIneract.text = baseInteractText + name;
    }
    public void DeactivateItemInteractText()
    {
        itemIneract.enabled = false;
    }

    public void ShowIventoryFullText()
    {
        inventoryFullText.gameObject.SetActive(true);
        inventoryFullText.color = Color.red;

        StopAllCoroutines();
        StartCoroutine(InventoryFullTextFader());
    }
    IEnumerator InventoryFullTextFader()
    {
        yield return new WaitForSeconds(1f);

        while(inventoryFullText.color.a >= .2f)
        {
            inventoryFullText.color = Color.Lerp(inventoryFullText.color, new Color(1,0,0,0), Time.deltaTime);
            yield return null;
        }
        inventoryFullText.gameObject.SetActive(false);
    }

    #endregion

    #region GameTimer
    public void SetGameTimeText(float timeRemaining)
    {
        string seconds = ((int)(timeRemaining % 60) > 9) ? (((int)(timeRemaining % 60)).ToString()) : "0" + ((int)(timeRemaining % 60));
        gameTimer.text = (int)(timeRemaining / 60f) + " : " + seconds;
    }
    #endregion

    #region GameOver

    public void PlayerDead()
    {
        PlayerDeathUI.SetActive(true);
        StartCoroutine(DelayNavigation(continueButton));
    }

    public void GameOver(CombatantActions[] combatants)
    {
        leaderBoard.SetActive(true);
        leaderBoard.GetComponent<Leaderboard>().PopulateLeaderboard(combatants);
        StartCoroutine(DelayNavigation(UIManager.instance.BackToMenuButton));
    }
    #endregion

    #region Navigation
    /// <summary>
    /// Seletcs something in navigation
    /// </summary>
    /// <param name="selectable"></param>
    /// <returns></returns>
    IEnumerator DelayNavigation(GameObject selectable)
    {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectable);

    }
    #endregion
}
