using UnityEngine;

namespace Core
{
	public static class UIHelper
	{
		private const float HalfFactor = 0.5f;

		public static Rect WindowPosition;

		public static void ResetWindowPosition()
		{
			WindowPosition = new Rect(Settings.ScreenW * HalfFactor - Settings.saveWindowSize.x * HalfFactor,
				Screen.height * HalfFactor - Settings.saveWindowSize.y * HalfFactor,
				Settings.saveWindowSize.x, Settings.saveWindowSize.y);
		}
	}
}