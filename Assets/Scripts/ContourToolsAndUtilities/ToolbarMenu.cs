using System;
using ContourEditorTool;
using UnityEngine;

namespace ContourToolsAndUtilities
{
	public class ToolbarMenu
	{
		public bool sticky = true;
		public Vector2 buttonSize = Vector2.one * 4;
		public float buttonMargin = 4;
		public Toolbar owner;
		//public Dictionary<GUIContent,Action>[] items;//If non-sticky, the Action of the first item is unnecessary, and will be executed upon the (temporary) unraveling of its contents, which is otherwise its only purpose.
		public Item[][] items;//If non-sticky, the Action of the first item is unnecessary, and will be executed upon the (temporary) unraveling of its contents, which is otherwise its only purpose.
		public Item ItemByTooltip(string tt)
		{
			for (int i = 0; i < items.Length; i++) for (int j = 0; j < items[i].Length; j++) if (items[i][j].buttonContent.tooltip == tt) return items[i][j];
			return null;
		}
		public int selectedCategory = 0, unfurledMenu = -1;
		public int[] selectedSubitems;
		//private bool unfurled=false,holdMode=false;
		//private bool holdMode=false;
		public bool unfurled { get { return unfurledMenu > -1; } }
		public Color selectedCategoryFarbe = Color.yellow;
		public KeyCode[] cycleShortcut;
		public void CheckShortcuts()
		{
			if (SRSUtilities.ComboDown(cycleShortcut)) Cycle(selectedCategory);
			for (int c = 0; c < items.Length; c++) for (int i = 0; i < items[c].Length; i++) if (!items[c][i].disabled && SRSUtilities.ComboDown(items[c][i].shortcut)) SelectItem(c, i/*,false*/);
		}
		public Vector2 Size(bool vertical = false)
		{
			Vector2 s = Vector2.one * buttonSize[vertical ? 0 : 1];
			//s[vertical?1:0]+=(buttonSize[vertical?1:0]+buttonMargin)*items.Length-buttonMargin;
			s[vertical ? 1 : 0] = buttonSize[vertical ? 1 : 0] * items.Length + buttonMargin * (items.Length - 1);
			//Debug.Log("Toolar.Menu.Size("+vertical+") items.Length: "+items.Length+", buttonSize: "+buttonSize+", buttonMargin: "+buttonMargin+", ergebnis: "+s);
			return s;
		}
		public class Item
		{
			public GUIContent buttonContent;
			public Action OnSelect;
			public KeyCode[] shortcut;
			public string info;
			public bool selected = false, disabled = false;
		}
		public ToolbarMenu(Item[][] items, bool sticky = false, Action<int> OnChangeCategory = null, GUIStyle style = null, Vector2 buttonSize = default(Vector2), float buttonMargin = 0)
		{
			Debug.Log("Toolar.Menu.Menu([" + items.Length + " items], " + style + "," + buttonSize + "," + buttonMargin + ")");
			this.items = items;
			//this.buttonSize=buttonSize>0?buttonSize:Toolbar.defaultButtonSize;
			this.buttonSize = buttonSize != default(Vector2) ? buttonSize : Toolbar.defaultButtonSize;
			this.buttonMargin = buttonMargin > 0 ? buttonMargin : Toolbar.defaultButtonMargin;
			this.sticky = sticky;
			selectedCategory = sticky ? 0 : -1;
			selectedSubitems = new int[items.Length];
			this.OnChangeCategory = OnChangeCategory;
		}
		public void Reset()
		{
			Debug.Log("Toolar.Menu.Reset()");
			for (int t = 0; t < selectedSubitems.Length; t++) selectedSubitems[t] = 0;
		}
		public void Cycle(int category)
		{
			selectedSubitems[category] = (selectedSubitems[category] + 1) % items[category].Length;
		}
		private Action<int> OnChangeCategory = null;

		private bool unfurling { get { return false; } }
		public Vector2 Draw(Vector2 p, bool vertical = false, bool direction = true)
		{
			if (ContourEditor.HideOldUI) return p;


			for (int i = 0; i < items.Length; i++, p[vertical ? 1 : 0] += (buttonSize[vertical ? 1 : 0] + buttonMargin) * (direction ? 1 : -1))
			{
				float anfang = sticky ? -selectedSubitems[i] * buttonSize[vertical ? 1 : 0] * (direction ? 1 : -1) : 0;
				if (unfurledMenu == i && unfurled) anfang = Mathf.Clamp(anfang, -p[vertical ? 0 : 1], (vertical ? Screen.width - buttonSize.x * items[i].Length : Screen.height - buttonSize.y * items[i].Length) - p[vertical ? 0 : 1]);

				for (int j = unfurled && unfurledMenu == i ? 0 : selectedSubitems[i]; j == selectedSubitems[i] || (unfurled && unfurledMenu == i && j < items[i].Length); j++)
				{
					GUI.color = sticky && selectedCategory == i && items[i][j].selected ? selectedCategoryFarbe : Color.white;
					GUI.enabled = !items[i][j].disabled;
					if (i == 3 && !Settings.IsRotation)
					{
						GUI.enabled = false;
					}
					if (GUI.Button(new Rect(p.x + (vertical ? buttonSize.x * j * (direction ? 1 : -1) + anfang : 0),
						    p.y + (!vertical ? buttonSize.y * j * (direction ? 1 : -1) + anfang : 0), buttonSize.x, buttonSize.y), items[i][j].buttonContent) && !unfurling)
					{
						Toolbar.clickedThisFrame = true;
						if (unfurledMenu != i) unfurledMenu = i;
						else SelectItem(i, j/*,true*/);
						Debug.Log("Button hit: " + items[i][j].buttonContent.tooltip + " i: " + i + " j: " + j);
						//Debug.Log("Hit button #" + i + "-" + j + ". selectedSubItems now: " + selectedSubitems.Stringify());
					}
                
				}
			}

			p[vertical ? 1 : 0] -= buttonSize[vertical ? 1 : 0];
			if (GUI.tooltip != "") Toolbar.DrawTooltip(GUI.tooltip);
			return p;
		}

		public void SelectItemFromUI(int i, int j)
		{
			Toolbar.clickedThisFrame = true;
			SelectItem(i, j);
			Debug.Log("Button hit: " + items[i][j].buttonContent.tooltip + " i: " + i + " j: " + j);
		}
    
		//Setting contour editor based on toolbar.
		public void SelectItem(int i, int j/*,bool unfurl=false*/)
		{
			if (selectedCategory != i && OnChangeCategory != null) OnChangeCategory(i);
			Debug.Log("Toolbar.Menu.SelectItem(" + i + "," + j +/*","+unfurl+*/") selectedCategory now: " + selectedCategory + ", unfurled: " + unfurled + ", sticky: " + sticky);

			if (unfurled) unfurledMenu = -1;//Unfurl(i,false);
			selectedCategory = !sticky && selectedCategory == i ? -1 : i;
			if (items[i][j].OnSelect != null) items[i][j].OnSelect();
			if (!sticky) items[i][j].selected = !items[i][j].selected;
			else if (!items[i][j].selected)
			{
				for (int k = 0; k < items[i].Length; k++) items[i][k].selected = k == j;
				selectedSubitems[i] = j;
			}
			if (items[i][j].info != null) Toolbar.toolMessage = items[i][j].info;
		}
	}
}