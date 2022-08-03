using UnityEngine;

namespace ContourToolsAndUtilities
{
	public class Toolbar : Draggable2D
	{
		public bool isNeedToShow;
		public ToolbarMenu[] menus;

		private void Awake()
		{
			isNeedToShow = false;
		}

		public void SetMenu(ToolbarMenu m)
		{
			SetMenu(new ToolbarMenu[] { m });
		}

		public void SetMenu(ToolbarMenu[] m)
		{
			menus = m;
		}

		public void Reset()
		{
			foreach (var menu in menus)
				menu.Reset();
		}

		public void OnGUI()
		{
			if (!isNeedToShow)
				return;

			GUI.depth = 1;
			GUI.color = Color.white;
			GUI.enabled = true;
		}

		public override void Update()
		{
			base.Update();

			foreach (var menu in menus)
				menu.CheckShortcuts();
		}
	}
}