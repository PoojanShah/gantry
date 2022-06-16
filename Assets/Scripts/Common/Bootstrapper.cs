using System.Linq;
using Configs;
using Core;
using UnityEngine;

namespace Common
{
	public class Bootstrapper : MonoBehaviour
	{
		private const byte QTS_POPUP_ID = 2;

		[SerializeField] private ScreensConfig _screensConfig;
		[SerializeField] private Transform _canvasTransform;

		private ICommonFactory _factory;
		private GameObject _currentScreen, _currentPopup;

		private void Awake()
		{
			_factory = new CommonFactory();

			ShowScreen(ScreenType.MainMenu);
		}

		private void ShowScreen(ScreenType type)
		{
			var screen = _screensConfig.Screens.FirstOrDefault(s => s.Type == type);

			if(screen == null)
				return;

			var instance = _factory.InstantiateObject<Transform>(screen.Prefab, _canvasTransform).gameObject;

			if (IsPopup(type))
			{
				_currentPopup = instance;
				//_currentPopup.Init();
			}
			else
			{
				Destroy(_currentScreen);

				//_currentScreen.Init();
				_currentScreen = instance;
			}
		}

		private bool IsPopup(ScreenType type) => (byte)type > QTS_POPUP_ID;
	}
}
