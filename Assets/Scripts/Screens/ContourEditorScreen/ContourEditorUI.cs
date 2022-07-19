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
		[SerializeField] private int _notInstrumentsBlockId = 2;

		[Header("Save popup")] 
		[SerializeField] private SavePopUp _savePopUp;
		[SerializeField] private Button _saveButton;
		[Header("Load popup")] 
		[SerializeField] private LoadPopUp _loadPopUp;
		[SerializeField] private Button _loadButton;

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
						line.ToolsTransform.gameObject.SetActive(false);
					}
				}
			};

			_saveButton.onClick.AddListener(() => 
				_savePopUp.gameObject.SetActive(true));
			
			_loadButton.onClick.AddListener(() =>
				_loadPopUp.gameObject.SetActive(true));
			
			_toolBarTransform.gameObject.SetActive(false);
			_savePopUp.gameObject.SetActive(false);
			_loadPopUp.gameObject.SetActive(false);
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
				
				if (block == _notInstrumentsBlockId)
					continue;
				
				button.Button.onClick.AddListener(() =>
				{
					line.lastButton = button;
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
				
				if (block == _notInstrumentsBlockId)
					return;
				if (line.ToolsTransform.gameObject.activeSelf)
					return;
				
				ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(line.LineNumber, line.lastButton.ID);
				ContourEditor.instance.MouseUp();
			});
			
			line.ToolsTransform.gameObject.SetActive(false);
			
			if (block != _notInstrumentsBlockId)
				line.MainButton.image.sprite = line.Instruments[0].Button.image.sprite;
		}

		private void InitToolButton(ToolButton button, int block, int line)
		{
			button.Button.onClick.AddListener(() =>
			{
				ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(line, button.ID);
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
		[HideInInspector] 
		public ToolButton lastButton;
	}
	
	[Serializable]
	public struct ToolButton
	{
		public int ID;
		public Button Button;
		public string ToolTip;
	}
}