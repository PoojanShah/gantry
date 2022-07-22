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
		[SerializeField] private TMP_Text _title;
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
			
			_hideLines += () =>
			{
				foreach (var block in _toolBar)
					foreach (var line in block.Lines)
						line.HideLine();
			};
			
			foreach (var block in _toolBar)
			{
				block.Init(_hideLines,
					SetToolTipByID,
					OnPointerExit,
					_currentInstrument
				);
			}

			foreach (var button in _additionalButtons)
			{
				button.handler.OnPointerEnterAction += () => 
					SetToolTipByID(button.ID.x, button.ID.y, button.ID.z);
				button.handler.OnPointerExitAction += () =>_title.text = "";
			}

			_currentInstrument.sprite = _toolBar[0]
				.Lines[0]
				.Instruments[0]
				.Button
				.image
				.sprite;
			
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

		private void SetToolTipByID(int block, int line, int id)
		{
			_title.text = ContourEditor.instance.toolbar.menus[block]
				.items[line][id]
				.buttonContent.tooltip;
		}
		
		private void OnPointerExit() =>
			_title.text = "";
	}

	[Serializable]
	public class DensityButton
	{
		public int Density;
		public Button Button;
	}
	
	[Serializable]
	public class ToolBarLinesBlock
	{
		public string Name; //just for inspector
		public int BlockNumber;
		public ToolBarLine[] Lines;

		public void Init(Action hideLines,
			Action<int, int, int> onPointerEnter,
			Action onPointerExit,
			Image currentInstrument)
		{
			foreach (var line in Lines)
			{
				line.Init(BlockNumber,
					hideLines,
					onPointerEnter,
					onPointerExit,
					currentInstrument);
			}
		}
	}
	
	[Serializable]
	public class ToolBarLine
	{
		[SerializeField] private int _lineNumber;
		[SerializeField] private Button _mainButton;
		[SerializeField] private ToolButton[] _instruments;
		[SerializeField] private Transform _toolsTransform;
		
		private ToolButton _lastButton;

		public ToolButton LastButton
		{
			get => _lastButton;
			set => _lastButton = value;
		}

		public Button MainButton => _mainButton;

		public int LineNumber => _lineNumber;

		public ToolButton[] Instruments => _instruments;

		public void Init(int block,
			Action hideLines,
			Action<int, int, int> onPointerEnter,
			Action onPointerExit,
			Image currentInstrument)
		{
			foreach (var button in _instruments)
			{
				button.Init(block,
					this,
					hideLines,
					onPointerEnter,
					onPointerExit,
					currentInstrument);
			}
			
			_mainButton.onClick.AddListener(() => MainButtonAction(block, hideLines, currentInstrument));
			
			_toolsTransform.gameObject.SetActive(false);

			if (block != BlockType.Functions.GetHashCode())
			{
				MainButton.image.sprite = _instruments[0].Button.image.sprite;
				_lastButton = _instruments[0];
			}
		}

		private void MainButtonAction(int block, Action hideLines, Image currentInstrument)
		{
			var isActive = _toolsTransform.gameObject.activeSelf;

			if (!isActive)
				hideLines.Invoke();

			_toolsTransform.gameObject.SetActive(!isActive);

			ContourEditor.instance.MouseUp();

			if (block != BlockType.Instrument.GetHashCode())
				return;
			if (_toolsTransform.gameObject.activeSelf)
				return;

			ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(_lineNumber, _lastButton.Id);
			ContourEditor.instance.MouseUp();

			currentInstrument.sprite = _lastButton.Button.image.sprite;
		}

		public void HideLine() =>
			_toolsTransform.gameObject.SetActive(false);
	}
	
	[Serializable]
	public class ToolButton
	{
		[SerializeField] private int _id;
		[SerializeField] private Button _button;

		public int Id => _id;

		public Button Button => _button;

		public void Init(int block,
			ToolBarLine line,
			Action hideLines,
			Action<int, int, int> onPointerEnter,
			Action onPointerExit,
			Image currentInstrument)
		{
			_button.onClick.AddListener(() => 
				SetButtonAction(block, line, hideLines, currentInstrument));
			
			var eventHandler = _button.gameObject.GetComponent<ButtonEventsHandler>();

			eventHandler.OnPointerEnterAction += () =>
				onPointerEnter?.Invoke(block, line.LineNumber, _id);
			eventHandler.OnPointerExitAction += () =>
				onPointerExit?.Invoke();
		}

		private void SetButtonAction(int block, ToolBarLine line, Action hideLines, Image currentInstrument)
		{
			Toolbar.clickedThisFrame = true;

			ContourEditor.instance.MouseUp();
			ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(line.LineNumber, _id);

			hideLines.Invoke();

			if (block != BlockType.Functions.GetHashCode())
				AddInstrumentInteraction(line, block, currentInstrument);
		}

		private void AddInstrumentInteraction(ToolBarLine line, int block, Image currentInstrument)
		{
			line.LastButton = this;
			line.MainButton.image.sprite = _button.image.sprite;

			if (block == BlockType.Instrument.GetHashCode())
				currentInstrument.sprite = _button.image.sprite;
		}
	}

	[Serializable]
	public class AdditionalButton
	{
		public Vector3Int ID;
		public ButtonEventsHandler handler;
	}
}