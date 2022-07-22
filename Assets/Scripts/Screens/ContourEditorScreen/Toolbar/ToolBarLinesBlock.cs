using System;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen.Toolbar
{
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
}