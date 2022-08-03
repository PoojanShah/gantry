using System;
using ContourEditorTool;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen.Toolbar
{
	[Serializable]
	public class ToolButton
	{
		[SerializeField] private int _id;
		[SerializeField] private Button _button;

		public int Id => _id;
		public Button Button => _button;

		public void Init(int block, ToolBarLine line, Action hideLines, Action<int, int, int> onPointerEnter,
			Action onPointerExit, Image currentInstrument)
		{
			_button.onClick.AddListener(() => SetButtonAction(block, line, hideLines, currentInstrument));

			var eventHandler = _button.gameObject.GetComponent<ButtonEventsHandler>();

			eventHandler.OnPointerEnterAction += () =>
				onPointerEnter?.Invoke(block, line.LineNumber, _id);
			eventHandler.OnPointerExitAction += () =>
				onPointerExit?.Invoke();
		}

		private void SetButtonAction(int block, ToolBarLine line, Action hideLines, Image currentInstrument)
		{
			ContourEditor.instance.MouseUp();
			ContourEditor.instance.toolbar.menus[block].SelectItemFromUI(block, line.LineNumber, _id);

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
}