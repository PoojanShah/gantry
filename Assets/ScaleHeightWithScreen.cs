using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class ScaleHeightWithScreen : MonoBehaviour
{
    public RectTransform PanelToScale;
    //Height that panel was initially created in unity....prob 625
    public int DefaulHeight;
    public float DefaultPanelHeight;
    private int _lastMeasuredHeight;
    private float _startingRectHeight;

    // Use this for initialization
    void Start()
    {
        UpdateSize();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Screen.height + " pH: " + PanelToScale.sizeDelta.y);
        if (_lastMeasuredHeight != Screen.height) UpdateSize();
    }

    private void UpdateSize()
    {
        _lastMeasuredHeight = Screen.height;
        float percentDiff = ((float) Screen.height/ (float) DefaulHeight);
        PanelToScale.sizeDelta = new Vector2(PanelToScale.sizeDelta.x, (DefaultPanelHeight * percentDiff));
        PanelToScale.offsetMax = new Vector2(PanelToScale.sizeDelta.x, 0);
        PanelToScale.offsetMin = new Vector2(0, -PanelToScale.sizeDelta.y);
        PanelToScale.ForceUpdateRectTransforms();
    }
}
