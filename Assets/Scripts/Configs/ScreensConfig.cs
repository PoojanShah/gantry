using UnityEngine;

namespace Configs
{
	public enum ScreenType : byte
	{
		MainMenu = 0,
		AdminMenu,
		LibraryMenu,
		PasswordPopup,
		ExitConfirmationPopup,
		Option
	}

	[System.Serializable]
	public class ScreenData
	{
		public ScreenType Type;
		public GameObject Prefab;
	}

	[CreateAssetMenu(fileName = "ScreensAndPopups", menuName = "Configs/Screens config")]
	public class ScreensConfig : ScriptableObject
	{
		public ScreenData[] Screens;
	}
}
