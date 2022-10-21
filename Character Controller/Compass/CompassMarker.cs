using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompassMarker : MonoBehaviour
{
    public bool displayDistance = true;
    public int priority = 0;
    public float viewDist = 50;
    public float minScale = .4f;
    [HideInInspector]
    public Sprite icon;
    [HideInInspector]
    public Image image;
    [HideInInspector]
    public Canvas canvas;
    [HideInInspector]
    public CanvasGroup canvasGroup;
    [HideInInspector]
    public TMP_Text distanceText;
    [HideInInspector]
    public RectTransform parentRectTransform;
    [HideInInspector]
    public RectTransform textRectTransform;
    [HideInInspector]
    public GameObject markerIcon;
    [HideInInspector]
    public float initialYPos;
    [HideInInspector]
    public float yPosOffset = 0;

    public Vector2 position
    {
        get { return new Vector2(transform.position.x, transform.position.z); }
    }

    public float distance = 0f;

    private void Update()
    {
        if(distanceText != null)
        {
            string distanceStr = distance.ToString("#.00") + "m";
            distanceText.text = distanceStr;
            
        }
            
    }
}
