using System;
using UnityEngine;

namespace Screens.ContourEditorScreen.Toolbar
{
	[Serializable]
	public class AdditionalButton
	{
		public Vector3Int ID;
		public ButtonEventsHandler handler;
	}
}