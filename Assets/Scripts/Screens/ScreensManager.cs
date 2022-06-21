using System;
using System.Linq;
using Configs;
using Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Screens
{
	public class ScreensManager
	{
		private const byte QTS_POPUP_ID = 2;
		private readonly Transform _canvasTransform;
		private readonly ScreensConfig _screensConfig;
		private readonly ICommonFactory _factory;

		private GameObject _currentScreen;

		public ScreensManager(ICommonFactory factory, ScreensConfig screensConfig, Transform canvasTransform)
		{
			_factory = factory;
			_screensConfig = screensConfig;
			_canvasTransform = canvasTransform;

			OpenWindow(ScreenType.MainMenu);
		}

		public GameObject ShowScreen(ScreenType type)
		{
			var screen = _screensConfig.Screens.FirstOrDefault(s => s.Type == type);

			if (screen == null)
				return null;

			var instance = _factory.InstantiateObject<Transform>(screen.Prefab, _canvasTransform).gameObject;

			if (IsPopup(type))
			{
				//_currentPopup.Init();
			}
			else
			{
				Object.Destroy(_currentScreen);

				//_currentScreen.Init();
				_currentScreen = instance;
			}

			return instance;
		}

		private static bool IsPopup(ScreenType type) => (byte)type > QTS_POPUP_ID;

		private void OpenWindow(ScreenType type)
		{
			var screen = ShowScreen(type);
			
			switch (type)
			{
				case ScreenType.MainMenu:
					InitMainMenu(screen);
					break;
				case ScreenType.AdminMenu:
					InitAdminMenu(screen);
					break;
				case ScreenType.LibraryMenu:
					InitLibrary(screen);
					break;
				case ScreenType.ExitConfirmationPopup:
					InitExitPopUp(screen);
					break;
				case ScreenType.PasswordPopup:
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private void InitMainMenu(GameObject screen)
		{
			var mainMenu = screen.GetComponent<MainMenu>();
			mainMenu.Init(() => OpenPasswordPopUp(() => OpenWindow(ScreenType.AdminMenu), Constants.CorrectAdminPass), 
				() => OpenWindow(ScreenType.ExitConfirmationPopup));
		}
		
		private void InitAdminMenu(GameObject screen)
		{
			var adminMenu = screen.GetComponent<AdminMenu>();
			adminMenu.Init(null, 
				null, 
				null, 
				() => OpenWindow(ScreenType.LibraryMenu),
				() => OpenWindow(ScreenType.MainMenu));
		}

		private void InitLibrary(GameObject screen)
		{
			var library = screen.GetComponent<Library>();
			library.Init(() => OpenWindow(ScreenType.AdminMenu));
		}
		
		private void InitExitPopUp(GameObject screen)
		{
			var exitPopUp = screen.GetComponent<ExitPopUp>();
			exitPopUp.Init(Application.Quit);
		}
		
		private void OpenPasswordPopUp(Action onContinue, string correctPass)
		{
			var screen = ShowScreen(ScreenType.PasswordPopup);
			var passwordPopUp = screen.GetComponent<PasswordPopUp>();
			passwordPopUp.Init((password) =>
			{
				if (password != correctPass)
					return;
				
				onContinue?.Invoke();
				Object.Destroy(screen);
			});
		}
	}
}