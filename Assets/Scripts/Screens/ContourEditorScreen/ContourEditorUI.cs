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
		[SerializeField] private int a;
		
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
		}
		
	}

	[Serializable]
	public struct DensityButton
	{
		public int Density;
		public Button Button;
	}
}