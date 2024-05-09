using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdown : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public CanvasGroup cvGroup;

    
    public void SetText(string text, Color color)
    {
        textMeshProUGUI.text = text;
        textMeshProUGUI.color = color;
    }


}
