using UnityEngine;

namespace Core
{
	public static class UIHelper
	{
		private const float HalfFactor = 0.5f;
		public static readonly Vector2 saveWindowSize = new(Settings.ScreenW * HalfFactor, Settings.ScreenH * HalfFactor);

		public static Rect WindowPosition;

		public static void ResetWindowPosition()
		{
			WindowPosition = new Rect(Settings.ScreenW * HalfFactor - saveWindowSize.x * HalfFactor,
				Screen.height * HalfFactor - saveWindowSize.y * HalfFactor,
				saveWindowSize.x, saveWindowSize.y);
		}
	}
}