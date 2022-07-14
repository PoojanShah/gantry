using System;
using ContourEditorTool;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen
{
	public class ContourEditorUI : MonoBehaviour
	{
		[Header("DensityPanel")]
		[SerializeField] private DensityButton[] _densityButtons;
		[SerializeField] private GameObject _setDensityPanel;
		[Header("Toolbar")]
		[SerializeField] private ToolButton test;
		
		private void Start()
		{
			foreach (var b in _densityButtons)
			{
				b.Button.onClick.AddListener(() =>
				{
					ContourEditor.instance.SetDensity(b.Density);
					
					_setDensityPanel.SetActive(false);
				});
			}
			
			test.Button.onClick.AddListener(() =>
			{
				ContourEditor.instance.toolbar.menus[1].SelectItemFromUI((int) test.number.x, (int) test.number.y);
				ContourEditor.instance.MouseUp();
			});
		}
		
	}

	[Serializable]
	public struct DensityButton
	{
		public int Density;
		public Button Button;
	}

	[Serializable]
	public struct ToolButton
	{
		public Vector2 number;
		public Button Button;
	}
}