using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    public GameObject gameRulesUI;
    public GameObject settingsUI;

    [Header("First Selected")]
    public GameObject settingsFirstSelected;
    public GameObject gameRulesFirstSelceted;
    public GameObject startButton;

    public GameObject menuUI;
    public GameObject recipiesUI;

    private EventSystem m_eventSystem;

    private void Start()
    {
        m_eventSystem = EventSystem.current;
        m_eventSystem.SetSelectedGameObject(startButton);

    }
    private void Update()
    {
        if (m_eventSystem.currentSelectedGameObject == null)
        {
            m_eventSystem.SetSelectedGameObject(m_eventSystem.firstSelectedGameObject);
        }
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    public void EnterGameRules()
    {
        menuUI.SetActive(false);
        //recipiesUI.SetActive(false);
        gameRulesUI.SetActive(true);
        m_eventSystem.SetSelectedGameObject(gameRulesFirstSelceted);
        StartCoroutine(DelaySelect(gameRulesFirstSelceted));

    }
    public void ExitGameRules()
    {
        menuUI.SetActive(true);
        //recipiesUI.SetActive(true);
        gameRulesUI.SetActive(false);
        StartCoroutine(DelaySelect(startButton));

    }
    public void EnterSettings()
    {
        menuUI.SetActive(false);
        //recipiesUI.SetActive(false);
        settingsUI.SetActive(true);
        StartCoroutine(DelaySelect(settingsFirstSelected));
    }
    //I just want to point out that it is stupid that i have to do this
    IEnumerator DelaySelect(GameObject slecetable)
    {
        yield return new WaitForEndOfFrame();
        m_eventSystem.SetSelectedGameObject(null);
        m_eventSystem.SetSelectedGameObject(slecetable);
        m_eventSystem.firstSelectedGameObject = slecetable;
    }
    public void ExitSettings()
    {
        menuUI.SetActive(true);
        //recipiesUI.SetActive(true);
        settingsUI.SetActive(false);
        StartCoroutine(DelaySelect(startButton));
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
