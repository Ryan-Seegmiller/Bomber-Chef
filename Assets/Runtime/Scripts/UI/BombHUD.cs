using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BombHUD : BaseHUD
{
    public Image fillImage;
    public Vector3 offset;
   [HideInInspector]public Transform parent;
    RectTransform rectTransform;
    Camera cam;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cam = Camera.main;
    }
    public void Update()
    {
        rectTransform.position = cam.WorldToScreenPoint(parent.position + offset);
    }

}
