using System;
using System.Linq;
using Common;
using Configs;
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
		private readonly Action<Action> _openEditorAction;
		private readonly Action<MediaContent> _playAction;
		private readonly Transform _canvasTransform;
		private readonly MainConfig _mainConfig;
		private readonly ICommonFactory _factory;
		private readonly MediaController _mediaController;

		private GameObject _currentScreen;

		public ScreensManager(ICommonFactory factory, MainConfig mainConfig, Transform canvasTransform,
			Action<MediaContent> playAction, Action<Action> openEditorAction, MediaController mediaController)
		{
			_factory = factory;
			_mainConfig = mainConfig;
			_canvasTransform = canvasTransform;
			_playAction = playAction;
			_openEditorAction = openEditorAction;
			_mediaController = mediaController;

			OpenWindow(ScreenType.MainMenu);
		}

		public GameObject ShowScreen(ScreenType type)
		{
			var screen = _mainConfig.ScreensConfig.Screens.FirstOrDefault(s => s.Type == type);

			if (screen == null)
				return null;

			var instance = _factory.InstantiateObject<Transform>(screen.Prefab, _canvasTransform).gameObject;

			Object.Destroy(_currentScreen);

			_currentScreen = instance;

			return instance;
		}

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
			mainMenu.Init(_mediaController, PlayVideo, () => OpenPasswordPopUp(() => OpenWindow(ScreenType.AdminMenu), PasswordType.Admin), 
				() => OpenWindow(ScreenType.ExitConfirmationPopup), _mainConfig.MediaItemPrefab, _factory);
		}

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
			_openEditorAction?.Invoke(() => OpenWindow(ScreenType.AdminMenu));

			Object.Destroy(_currentScreen);
		}

	}
}