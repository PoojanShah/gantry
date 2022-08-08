using System.Linq;
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
		OptionsMenu,
		LibraryOptions,
		MainMenuAndroid
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
		[SerializeField] private ScreenData[] Screens;

		public GameObject GetScreenPrefab(ScreenType type)
		{
#if UNITY_ANDROID
			if (type == ScreenType.MainMenu)
				type = ScreenType.MainMenuAndroid;
#endif
			var screen = Screens.FirstOrDefault(s => s.Type == type);

			return screen?.Prefab;
		}
	}
}
