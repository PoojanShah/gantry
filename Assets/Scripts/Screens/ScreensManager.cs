using System;
using System.Linq;
using Common;
using Configs;
using Core;
using Library;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Screens
{
	public class ScreensManager
	{
		private const byte QTS_POPUP_ID = 2;
		private readonly Transform _canvasTransform;
		private readonly MainConfig _mainConfig;
		private readonly ICommonFactory _factory;
		private readonly Action<int> _playAction;

		private GameObject _currentScreen;

		public ScreensManager(ICommonFactory factory, MainConfig mainConfig, Transform canvasTransform, Action<int> playAction)
		{
			_factory = factory;
			_mainConfig = mainConfig;
			_canvasTransform = canvasTransform;
			_playAction = playAction;

			OpenWindow(ScreenType.MainMenu);
		}

		public GameObject ShowScreen(ScreenType type)
		{
			var screen = _mainConfig.ScreensConfig.Screens.FirstOrDefault(s => s.Type == type);

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
				case ScreenType.OptionsMenu:
					InitOptions(screen);
					break;
				case ScreenType.PasswordPopup:
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private void InitMainMenu(GameObject screen)
		{
			var mainMenu = screen.GetComponent<MainMenu>();
			mainMenu.Init(PlayVideoAction, () => OpenPasswordPopUp(() => OpenWindow(ScreenType.AdminMenu), PasswordType.Admin), 
				() => OpenWindow(ScreenType.ExitConfirmationPopup), _mainConfig.VideosConfig, _factory);
		}
		
		private void InitAdminMenu(GameObject screen)
		{
			var adminMenu = screen.GetComponent<AdminMenu>();
			adminMenu.Init(PlayVideoAction, 
				null, 
				() => OpenPasswordPopUp(() => OpenWindow(ScreenType.OptionsMenu), PasswordType.SuperAdmin), 
				() => OpenWindow(ScreenType.LibraryMenu),
				() => OpenWindow(ScreenType.MainMenu));
		}

		private void InitLibrary(GameObject screen)
		{
			var library = screen.GetComponent<LibraryScreen>();
			library.Init(() => OpenWindow(ScreenType.AdminMenu));
		}

		private void InitOptions(GameObject screen)
		{
			var options = screen.GetComponent<OptionsMenu>();
			options.Init();
		}
		
		private void InitExitPopUp(GameObject screen)
		{
			var exitPopUp = screen.GetComponent<ExitPopUp>();
			exitPopUp.Init(Application.Quit);
		}
		
		private void OpenPasswordPopUp(Action onContinue, PasswordType type)
		{
			var screen = ShowScreen(ScreenType.PasswordPopup);
			var passwordPopUp = screen.GetComponent<PasswordPopUp>();

			if (LoginHelper.IsLoggedInByType(type))
			{
				onContinue?.Invoke();
				Object.Destroy(screen);
			}
			
			passwordPopUp.Init((password) =>
			{
				if (password != LoginHelper.GetPasswordByType(type))
					return;
				
				LoginHelper.LogInByType(type);
				onContinue?.Invoke();
				Object.Destroy(screen);
			}, type);
		}

		private void PlayVideoAction(int videoId)
		{
			_playAction?.Invoke(videoId);

			Object.Destroy(_currentScreen);
		}
	}
}