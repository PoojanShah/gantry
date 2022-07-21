using System;
using ContourEditorTool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen
{
	enum BlockType : byte
	{
		Instrument = 0,
		Pattern = 1,
		Functions = 2,
	}
	
	public class ContourEditorUI : MonoBehaviour
	{
		[Header("DensityPanel")]
		[SerializeField] private DensityButton[] _densityButtons;
		[SerializeField] private GameObject _densityPanel;
		[Header("Toolbar")] 
		[SerializeField] private ToolBarLinesBlock[] _toolBar;
		[SerializeField] private Transform _toolBarTransform;
		[SerializeField] private TextMeshProUGUI _title;
		[SerializeField] private Image _currentInstrument;

		[Header("Save popup")] 
		[SerializeField] private SavePopUp _savePopUp;
		[SerializeField] private Button _saveButton;
		[Header("Load popup")] 
		[SerializeField] private LoadPopUp _loadPopUp;
		[SerializeField] private Button _loadButton;

		[Header("Additional buttons")] 
		[SerializeField] private AdditionalButton[] _additionalButtons;

		private Action _hideLines;

		private void Start()
		{
			foreach (var b in _densityButtons)
			{
				b.Button.onClick.AddListener(() =>
				{
					ContourEditor.instance.SetDensity(b.Density);
					
					_densityPanel.SetActive(false);
					_toolBarTransform.gameObject.SetActive(true);
				});
			}

			foreach (var block in _toolBar)
			{
				InitToolBarBlock(block);
			}

			foreach (var button in _additionalButtons)
			{
				button.handler._onPointerEnterAction += () => 
					SetToolTipByID(button.ID.x, button.ID.y, button.ID.z);
				button.handler._onPointerExitAction += () =>_title.text = "";
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

			_currentInstrument.sprite = _toolBar[0]
				.Lines[0]
				.Instruments[0]
				.Button.image.sprite;
			
			_saveButton.onClick.AddListener(() => 
				_savePopUp.gameObject.SetActive(true));
			
			_loadButton.onClick.AddListener(() =>
				_loadPopUp.gameObject.SetActive(true));
			
			_toolBarTransform.gameObject.SetActive(false);
			_savePopUp.gameObject.SetActive(false);
			_loadPopUp.gameObject.SetActive(false);
		}

		public void ShowDensityPanel()
		{
			_densityPanel.gameObject.SetActive(true);
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

				if (block == BlockType.Functions.GetHashCode())
					continue;
				
				button.Button.onClick.AddListener(() =>
				{
					line.lastButton = button;
					line.MainButton.image.sprite = button.Button.image.sprite;

					if (block == BlockType.Instrument.GetHashCode())
						_currentInstrument.sprite = button.Button.image.sprite;
				});
			}
			
			line.MainButton.onClick.AddListener(() =>
			{
				var isActive = line.ToolsTransform.gameObject.activeSelf;
				
				if (!isActive)
					_hideLines.Invoke();

				line.ToolsTransform.gameObject.SetActive(!isActive);

				ContourEditor.instance.MouseUp();
				
				if (block != BlockType.Instrument.GetHashCode())
					return;
				if (line.ToolsTransform.gameObject.activeSelf)
					return;
				
				ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(line.LineNumber, line.lastButton.ID);
				ContourEditor.instance.MouseUp();
			});
			
			line.ToolsTransform.gameObject.SetActive(false);
			
			if (block != BlockType.Functions.GetHashCode())
				line.MainButton.image.sprite = line.Instruments[0].Button.image.sprite;
		}

		private void InitToolButton(ToolButton button, int block, int line)
		{
			button.Button.onClick.AddListener(() =>
			{
				Toolbar.clickedThisFrame = true;
				
				ContourEditor.instance.MouseUp();
				ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(line, button.ID);
			});
			
			var eventHandler = button.Button.gameObject.GetComponent<ButtonEventsHandler>();

			eventHandler._onPointerEnterAction += () => SetToolTipByID(block, line, button.ID);
			eventHandler._onPointerExitAction += () =>_title.text = "";
		}

		private void SetToolTipByID(int block, int line, int id)
		{
			_title.text = ContourEditor.instance.toolbar.menus[block]
				.items[line][id]
				.buttonContent.tooltip;
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

	[Serializable]
	public struct AdditionalButton
	{
		public Vector3Int ID;
		public ButtonEventsHandler handler;
	}
}