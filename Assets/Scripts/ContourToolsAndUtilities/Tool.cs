using System;
using System.Collections.Generic;
using UnityEngine;

public class Tool
{
	public int SelectedSubtoolId = 0;
	public Action<Vector2> OnMouseDown, OnDrag, Draw, OnFinishDrag, OnSingleClick;
	public Dictionary<KeyCode, Action> keyDowns;
}
