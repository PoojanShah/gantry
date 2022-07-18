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
		[SerializeField] private ToolBarLinesBlock[] _toolBar;
		[SerializeField] private Transform _toolBarTransform;
		[SerializeField] private int _instrumentsBlockId = 0;

		private Action _hideLines;
		
		private void Start()
		{
			foreach (var b in _densityButtons)
			{
				b.Button.onClick.AddListener(() =>
				{
					ContourEditor.instance.SetDensity(b.Density);
					
					_setDensityPanel.SetActive(false);
					_toolBarTransform.gameObject.SetActive(true);
				});
			}

			foreach (var block in _toolBar)
			{
				InitToolBarBlock(block);
			}

			_hideLines += () =>
			{
				foreach (var block in _toolBar)
				{
					foreach (var line in block.Lines)
					{
						HideButtonsInLine(line);
					}
				}
			};

			_toolBarTransform.gameObject.SetActive(false);
		}

		private void InitToolBarBlock(ToolBarLinesBlock block)
		{
			foreach (var line in block.Lines)
			{
				InitToolLine(line, block.BlockNumber);
			}
		}

		private void InitToolLine(ToolBarLine line, int block)
		{
			foreach (var button in line.Instruments)
			{
				InitToolButton(button, block, line.LineNumber);
				button.Button.onClick.AddListener(() => _hideLines.Invoke());
				
				if (block != _instrumentsBlockId)
					continue;
				
				button.Button.onClick.AddListener(() =>
				{
					line.MainButton.image.sprite = button.Button.image.sprite;
				});
			}
			
			line.MainButton.onClick.AddListener(() =>
			{
				var isActive = line.ToolsTransform.gameObject.activeSelf;
				if (!isActive)
					_hideLines.Invoke();

				line.ToolsTransform.gameObject.SetActive(!isActive);

				ContourEditor.instance.MouseUp();
			});
			
			HideButtonsInLine(line);
		}

		private void InitToolButton(ToolButton button, int block, int line)
		{
			button.Button.onClick.AddListener(() =>
			{
				ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(line, button.ID);
				ContourEditor.instance.MouseUp();
			});
		}

		private void HideButtonsInLine(ToolBarLine line)
		{
			line.ToolsTransform.gameObject.SetActive(false);
			//line.isOpen = false;
		}
	}

	[Serializable]
	public struct DensityButton
	{
		public int Density;
		public Button Button;
	}
	
	[Serializable]
	public struct ToolBarLinesBlock
	{
		public string Name; //just for inspector
		public int BlockNumber;
		public ToolBarLine[] Lines;
	}
	
	[Serializable]
	public struct ToolBarLine
	{
		public int LineNumber;
		public Button MainButton;
		public ToolButton[] Instruments;
		public Transform ToolsTransform;
	}
	
	[Serializable]
	public struct ToolButton
	{
		public int ID;
		public Button Button;
		public string ToolTip;
	}
}