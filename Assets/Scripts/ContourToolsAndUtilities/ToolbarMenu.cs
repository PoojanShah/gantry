using System;
using ContourEditorTool;
using UnityEngine;

namespace ContourToolsAndUtilities
{
	public class ToolbarMenu
	{
		public bool sticky = true;
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
		private Action<int> OnChangeCategory = null;
		
		public void CheckShortcuts()
		{
			if (SRSUtilities.ComboDown(cycleShortcut)) Cycle(selectedCategory);}
		
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

		public void SelectItemFromUI(int block, int i, int j)
		{
			SelectItem(block, i, j);
			//Debug.Log("Button hit: " + items[i][j].buttonContent.tooltip + " i: " + i + " j: " + j);
		}
    
		//Setting contour editor based on toolbar.
		public void SelectItem(int block, int i, int j/*,bool unfurl=false*/)
		{
			if (selectedCategory != i && OnChangeCategory != null) OnChangeCategory(i);

			if (unfurled) unfurledMenu = -1;//Unfurl(i,false);
			selectedCategory = !sticky && selectedCategory == i ? -1 : i;
			
			ContourEditor.instance.InstrumentAction(block, i, j);
			/*if (items[i][j].OnSelect != null) items[i][j].OnSelect();
			if (!sticky) items[i][j].selected = !items[i][j].selected;
			else if (!items[i][j].selected)
			{
				for (int k = 0; k < items[i].Length; k++) items[i][k].selected = k == j;
				selectedSubitems[i] = j;
			}
			if (items[i][j].info != null)
			{
			}*/
		}
	}
}