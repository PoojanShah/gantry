using System;
using System.Collections.Generic;
using UnityEngine;

public class Tool
{
    public int selectedSubtool = 0;
    public Action<Vector2> OnMouseDown, OnDrag, Draw, OnFinishDrag, OnSingleClick;
    public Dictionary<KeyCode, Action> keyDowns;
}