using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestingText : MonoBehaviour
{
    public int order = 0;
    public TMP_Text text;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        text.canvas.sortingOrder = order;
    }

    private void Update()
    {
        text.canvas.sortingOrder = order;
    }
}
