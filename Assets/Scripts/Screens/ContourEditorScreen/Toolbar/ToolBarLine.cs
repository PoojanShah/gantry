using System;
using ContourEditorTool;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen.Toolbar
{
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
}