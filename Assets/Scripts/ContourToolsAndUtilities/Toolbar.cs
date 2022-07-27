﻿using UnityEngine;

namespace ContourToolsAndUtilities
{
	public class Toolbar : Draggable2D
	{
		public bool isNeedToShow;
		public InfoDisplay info;
		public ToolbarMenu[] menus;

		private void Awake()
		{
			info = GetComponent<InfoDisplay>();

			isNeedToShow = false;
			info.isNeedToShow = false;
		}

		private void Start()
		{
			menus[0].SelectItem(0, 0);
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