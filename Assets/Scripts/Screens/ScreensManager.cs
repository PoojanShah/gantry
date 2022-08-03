using System;
using Common;
using Configs;
using ContourEditorTool;
using Core;
using Library;
using Media;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Screens
{
	public class ScreensManager
	{
		private const byte QTS_POPUP_ID = 2;
		private readonly Action<MediaContent> _playAction;
		private readonly ContourEditorController _contourEditorController;
		private readonly Transform _canvasTransform;
		private readonly MainConfig _mainConfig;
		private readonly ICommonFactory _factory;
		private readonly MediaController _mediaController;

		private GameObject _currentScreen;
		private MainMenu _menu;

		public ScreensManager(ICommonFactory factory, MainConfig mainConfig, Transform canvasTransform,
			Action<MediaContent> playAction, ContourEditorController contourEditorController, MediaController mediaController)
		{
			_factory = factory;
			_mainConfig = mainConfig;
			_canvasTransform = canvasTransform;
			_playAction = playAction;
			_contourEditorController = contourEditorController;
			_mediaController = mediaController;

			OpenWindow(ScreenType.MainMenu);
		}

		public GameObject ShowScreen(ScreenType type)
		{
			var screenPrefab = _mainConfig.ScreensConfig.GetScreenPrefab(type);

			if (screenPrefab == null)
				return null;

			var instance = _factory.InstantiateObject<Transform>(screenPrefab, _canvasTransform).gameObject;

			if (!IsPopup(type))
			{
				Object.Destroy(_currentScreen);

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
#if UNITY_STANDALONE_WIN
			var mainMenu = screen.GetComponent<MainMenu>();
			mainMenu.Init(_mediaController, PlayVideo,
				() => OpenPasswordPopUp(() => OpenWindow(ScreenType.AdminMenu), PasswordType.Admin), 
				Application.Quit, _mainConfig.MediaItemPrefab, _factory);

			_menu = mainMenu;
#elif UNITY_ANDROID
			var mainMenu = screen.GetComponent<MainMenuAndroid>();
			mainMenu.Init(Application.Quit, _mainConfig.MediaItemPrefab, _factory);
#endif
		}

#if UNITY_STANDALONE_WIN
		public void PlayVideoById(int id)
		{
			_menu.PlayById(id);
		}
#endif

		public void ReloadMediaItems(MediaContent[] media, ICommonFactory factory, GameObject mediaPrefab, Action<MediaContent> playVideoAction)
		{
			var mainMenu = _currentScreen.GetComponent<MainMenu>();
			mainMenu.ClearMediaItems();
			mainMenu.InitMediaItems(media, factory, mediaPrefab, PlayVideo);
		}

		public void SetMediaInteractable()
		{
			var mainMenu = _currentScreen.GetComponent<MainMenu>();
			mainMenu.SetMediaInteractable();
		}
		
		private void InitAdminMenu(GameObject screen)
		{
			var adminMenu = screen.GetComponent<AdminMenu>();
			adminMenu.Init(OpenPatternsEditor, 
				() => OpenPasswordPopUp(() => OpenWindow(ScreenType.OptionsMenu), PasswordType.SuperAdmin), 
				() => OpenWindow(ScreenType.LibraryMenu),
				() => OpenWindow(ScreenType.MainMenu));
		}

		private void InitLibrary(GameObject screen)
		{
			var library = screen.GetComponent<LibraryScreen>();
			library.Init(_factory, () => OpenWindow(ScreenType.AdminMenu));
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

		private void PlayVideo(MediaContent content)
		{
			Object.Destroy(_currentScreen);

			_playAction?.Invoke(content);
		}

		private void OpenPatternsEditor()
		{
			_contourEditorController?.Show(() => OpenWindow(ScreenType.AdminMenu));

			Object.Destroy(_currentScreen);
		}
	}
}