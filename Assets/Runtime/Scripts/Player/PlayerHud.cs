using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PlayerHud : BaseHUD
{
    #region Declerations

    #region Inspector set gameobjects
    [Header("Positioning")]
    public RectTransform heartHolder;

    [Header("Prefabs")]
    public GameObject deathImage;
    public GameObject heart;
    public GridLayoutGroup layoutGroup;
    #endregion

    #region Text
    [Header("Text")]
    public TextMeshProUGUI nameText;
    [SerializeField] private string baseKillText;
    [SerializeField]private TextMeshProUGUI killText;
    #endregion

    #region Start Animation
    [Header("Start Animation")]
    private CanvasGroup cvGroup;
    private RectTransform rTransform;
    public float offScreenOffestPos = 50f;
    private Vector3 startPoition;
    private Vector3 endPostion;
    private bool fadedIn = true;
    private bool fadedOut = true;
    private float fadeDurationTime;
    public float fadeDuration = 1;
    #endregion

    #region Health
    [Header("Heart Shake")]
    public float heartShakeDuration;
    private bool heartsShook = true;
    private float heartShakeTime;
    public float heartShakeCount = 20;
    public float heartRotationAmount = 20;
    private int damageTaken;
    Stack<Image> UIHeartsStack;
    private Image[] UIHearts;
    #endregion

    #region Respawn Bar
    public Image respawnImage;
    #endregion

    #endregion

    #region MonoBehaviours
    public void Start()
    {
        rTransform = GetComponent<RectTransform>();
        cvGroup = GetComponent<CanvasGroup>();
        cvGroup.alpha = 0;
        StartCoroutine(DelayedStart());
    }
    public void Update()
    {
        FadeIn();
        FadeOut();
        ShakeHearts();
    }
    #endregion

    #region Setup
    public void SetNames(string name, Color color)
    {
        ChangeName(name, nameText);
        nameText.color = color;
    }
    public void SetupLives(int liveCount)
    {
        UIHearts = new Image[liveCount];
        UIHeartsStack = new Stack<Image>(liveCount);

        for (int i = 0; i < UIHearts.Length; i++)
        {   //Creates a heart for the hud
            GameObject hudHeart = Instantiate(heart);
            hudHeart.transform.SetParent(heartHolder);
            hudHeart.GetComponentInChildren<Heart>().removeHeart += PopHeart;

            UIHearts[i] = hudHeart.GetComponentInChildren<Image>();
            UIHeartsStack.Push(UIHearts[i]);      
        }
    }
    #endregion

    #region Display text and health
    /// <summary>
    /// Removes a heart
    /// </summary>
    public void PopHeart()
    {
        if (UIHeartsStack.Peek() == null) { return; }
        Image heart = UIHeartsStack.Pop();
        heart.gameObject.SetActive(false);
        for (int i = 0; i < UIHearts.Length; i++)
        {
            if(heart == UIHearts[i])
            {
                UIHearts[i] = null;
            }
        }
       
    }
    /// <summary>
    /// Starts the removal of health
    /// </summary>
    /// <param name="health"></param>
    public void RemoveHealth(int health)
    {
        heartShakeTime = 0;
        heartsShook = false;
        damageTaken += health;
       
    }
    /// <summary>
    /// Sets the kill text
    /// </summary>
    /// <param name="killAmount"></param>
    public void KillCounter(int killAmount)
    {
        killText.text = baseKillText + killAmount.ToString();
    }
    public void Death()
    {
        deathImage.gameObject.SetActive(true);
        fadedOut = false;
        fadeDurationTime = 0;
    }
    #endregion
    
    #region Animated UI
    /// <summary>
    /// Shakes the hearts and pops one out when damaged 
    /// </summary>
    public void ShakeHearts()
    {
        if (heartsShook) { return; }
        if (UIHearts == null) { return; }

        if (heartShakeTime < heartShakeDuration)
        {
            heartShakeTime += Time.deltaTime;
            //Loops through the hearts and gives thema slight shake
            for (int i = 0; i < UIHearts.Length; i++)
            {
                if (UIHearts[i] == null) { continue; }
                RectTransform rectTR = UIHearts[i].GetComponent<RectTransform>();
                rectTR.rotation = Quaternion.Euler(0, 0, Mathf.Sin(heartShakeTime * (heartShakeCount * heartShakeDuration)) * heartRotationAmount);
            }
            return;
        }

        ResetHeartsAddPop();
    }
    public void ResetHeartsAddPop()
    {
        //Reset Rotation
        for (int i = 0; i < UIHearts.Length; i++)
        {
            if (UIHearts[i] == null) { continue; }
            RectTransform rectTR = UIHearts[i].GetComponent<RectTransform>();
            rectTR.rotation = Quaternion.identity;
        }
        heartsShook = true;
        if (UIHeartsStack.Peek() == null) { return; }//If there is none dont bother

        //Gets rid of all the hearts
        for (int i = UIHearts.Length - 1; i >= 0; i--)
        {
            if (damageTaken == 0) { break; }
            if (UIHearts[i] != null)
            {
                damageTaken--;
                UIHearts[i].GetComponent<Animation>().Play();
            }
        }
    }

    /// <summary>
    /// Fades in the canvases
    /// </summary>
    public void FadeIn()
    {
        if (fadedIn) { return; }

        fadeDurationTime += Time.deltaTime;
        if(fadeDurationTime < fadeDuration)
        {
            cvGroup.alpha = Mathf.Lerp(0, 1, fadeDurationTime / fadeDuration);
            rTransform.position = Vector3.Lerp(startPoition, endPostion, fadeDurationTime / fadeDuration);//Brings them in from the side
            return;
        }
        
        cvGroup.alpha = 1;
        fadedIn = true;
        
    }
    /// <summary>
    /// Fades out the canvases
    /// </summary>
    public void FadeOut()
    {
        if (fadedOut) { return; }
        fadeDurationTime += Time.deltaTime;
        if (fadeDurationTime < fadeDuration)
        {
            cvGroup.alpha = Mathf.Lerp(1, 0, fadeDurationTime / fadeDuration);
            rTransform.position = Vector3.Lerp(endPostion, startPoition, fadeDurationTime / fadeDuration);//Brings them out from the side
            return;
        }

        cvGroup.alpha = 0;
        fadedIn = true;
        
    }
    #endregion

    #region Respawn Bar
    public void FillRespawnBar(float time, float totalTime)
    {
        respawnImage.fillAmount =  time / totalTime;
    }

    #endregion

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        endPostion = rTransform.position;
        startPoition = new Vector3(endPostion.x - offScreenOffestPos, endPostion.y, endPostion.z);
        fadeDurationTime = 0;
        fadedIn = false;

    }
}
