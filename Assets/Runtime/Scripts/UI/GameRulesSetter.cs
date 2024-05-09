using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameRulesSetter : MonoBehaviour
{
    public GameObject healthSlider;
    public GameObject cookTimeSlider;
    public GameObject respawnTimeslider;
    public GameObject gameTimeSlider;
    public GameObject[] levelSelectors = new GameObject[3];
    public Color normalLevelSelectColor;

    [Header("Base Text")]
    public string baseHealthText = string.Empty;
    public string baseCookText = string.Empty;
    public string baseRespawnTimeText = string.Empty;
    public string baseGameTimeText = string.Empty;


    Dictionary<string, GameObject> gameRules = new Dictionary<string, GameObject>();

  
    private void Start()
    {
        Initalize();
        gameObject.SetActive(false);
    }
    public void Initalize()
    {
        gameRules = new Dictionary<string, GameObject>()
        {
            { GameRules.healthGameRuleKey, healthSlider},
            {GameRules.CookTimeKey, cookTimeSlider },
            {GameRules.respawnTimeKey, respawnTimeslider },
            {GameRules.durationKey, gameTimeSlider }
        };

        if (!PlayerPrefs.HasKey(GameRules.levelSelectGameRuleKey))
        {
            PlayerPrefs.SetInt(GameRules.levelSelectGameRuleKey, 0);
        }
        foreach(KeyValuePair<string, GameObject> rule in gameRules)
        {
            if (PlayerPrefs.HasKey(rule.Key))
            {
                LoadGameRules(rule.Key, rule.Value);
            }
            else
            {
                SetGameRule(rule.Key, rule.Value);
            }
        }
        LoadLevelSelect();
    }
    //Sets the slider game rules
    public void SetGameRule(string key, GameObject value)
    {
        Slider slider = value.GetComponentInChildren<Slider>(true);
        if (slider != null)
        {
            float rule = slider.value;
            PlayerPrefs.SetFloat(key, rule);
            TextMeshProUGUI text = value.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                SetSliderText(key, rule, text);
            }
            if(slider.onValueChanged.GetPersistentEventCount() <= 0)
            {
                slider.onValueChanged.AddListener(delegate {
                    SetGameRule(key, value);
                });
            }
        }
    }
    //For level select
    public void SetGameRule(string key, int value, GameObject obj)
    {
        foreach(GameObject buttonObj in levelSelectors)
        {
            Image buttonImg = buttonObj.GetComponentInChildren<Image>(true);
            buttonImg.color = normalLevelSelectColor;

        }
        Image img = obj.GetComponentInChildren<Image>(true);
        img.color = Color.white;
        PlayerPrefs.SetInt(key, value);
    }
    private void SetSliderText(string key, float value, TextMeshProUGUI text)
    {
        if (key == GameRules.healthGameRuleKey)
        {
            text.text = baseHealthText + value;
        }
        else if(key == GameRules.CookTimeKey)
        {
            text.text = baseCookText + value;
        }
        else if (key == GameRules.respawnTimeKey)
        {
            text.text = baseRespawnTimeText + value;
        }
        else if (key == GameRules.durationKey)
        {
            text.text = baseGameTimeText + value;
        }
    }
    //Loads the health and cook time
    void LoadGameRules(string key, GameObject value)
    {
        Slider slider = value.GetComponentInChildren<Slider>(true);
        if (slider != null)
        {
            float rule = PlayerPrefs.GetFloat(key);
            slider.value = rule;
            TextMeshProUGUI text = value.GetComponentInChildren<TextMeshProUGUI>(true);
            if(text != null)
            { 
                SetSliderText(key, rule, text);
            }
            slider.onValueChanged.AddListener(delegate { 
                SetGameRule(key, value); 
            });
            return;
        }
    }
    //Loads the level that was saved
    void LoadLevelSelect()
    {
        int index = PlayerPrefs.GetInt(GameRules.levelSelectGameRuleKey);
       

        for (int i = 0; i < levelSelectors.Length; i++)
        {
            Button button = levelSelectors[i].GetComponentInChildren<Button>(true);
            if(index == i)
            {
                Image img = levelSelectors[i].GetComponentInChildren<Image>(true);
                img.color = Color.white;
            }
            GameObject obj = levelSelectors[i];
            int value = i;

            button.onClick.AddListener(delegate { SetGameRule(GameRules.levelSelectGameRuleKey, value, obj); });
        }

    }
    private void OnDestroy()
    {
        foreach(KeyValuePair<string, GameObject> values in gameRules)
        {
            print(values.Key + ": " + PlayerPrefs.GetFloat(values.Key));
        }
    }
#if UNITY_EDITOR
    [ContextMenu("Reset player prefs")]
    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    #endif

}
