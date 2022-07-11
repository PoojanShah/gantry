using UnityEngine;
using Debug = UnityEngine.Debug;

public class Menu : MonoBehaviour
{

	private static Vector2 windowDragOffset = -Vector2.one;

	public GameObject menuBackground;
	public GUISkin gantrySkin;
	public Texture2D illuminationsHeader, categoryFooter, mediaFooter, backArrow, blankScreen, adminButton;
	public static Rect windowPosition;

	public static bool DraggingWindow
	{
		get => windowDragOffset != -Vector2.one;
		set
		{
			windowDragOffset = value ? SRSUtilities.adjustedFlipped - windowPosition.position : -Vector2.one;
			Debug.Log("windowDragOffset set to: " + windowDragOffset + " (value: " + value + ")");
		}
	}

	public static void ResetWindowPosition()
	{
		Debug.Log("Menu.ResetWindowPosition() Settings.menuScreenW: " + Settings.ScreenW + ", saveWindowSize.x: " +
		          Settings.saveWindowSize.x);

		windowPosition = new Rect(Settings.ScreenW * 0.5f - Settings.saveWindowSize.x * 0.5f,
			Screen.height * 0.5f - Settings.saveWindowSize.y * 0.5f, Settings.saveWindowSize.x, Settings.saveWindowSize.y);
	}
}
