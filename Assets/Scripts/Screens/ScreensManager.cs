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
		private readonly Action<Action> _openEditorAction;
		private readonly Action<int> _playAction;
		private readonly Transform _canvasTransform;
		private readonly MainConfig _mainConfig;
		private readonly ICommonFactory _factory;

		private GameObject _currentScreen;

		public ScreensManager(ICommonFactory factory, MainConfig mainConfig, Transform canvasTransform,
			Action<int> playAction, Action<Action> openEditorAction)
		{
			_factory = factory;
			_mainConfig = mainConfig;
			_canvasTransform = canvasTransform;
			_playAction = playAction;
			_openEditorAction = openEditorAction;

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

		public void OpenWindow(ScreenType type)
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
			mainMenu.Init(PlayVideo, () => OpenPasswordPopUp(() => OpenWindow(ScreenType.AdminMenu), PasswordType.Admin), 
				() => OpenWindow(ScreenType.ExitConfirmationPopup), _mainConfig.VideosConfig, _factory);
		}
		
		private void InitAdminMenu(GameObject screen)
		{
			var adminMenu = screen.GetComponent<AdminMenu>();
			adminMenu.Init(PlayVideo, 
				OpenPatternsEditor, 
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

		private void PlayVideo(int videoId)
		{
			_playAction?.Invoke(videoId);

			Object.Destroy(_currentScreen);
		}

		private void OpenPatternsEditor()
		{
			_openEditorAction?.Invoke(() => OpenWindow(ScreenType.AdminMenu));

			Object.Destroy(_currentScreen);
		}

	}
}