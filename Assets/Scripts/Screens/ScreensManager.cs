using System;
using Common;
using Configs;
using ContourEditorTool;
using Core;
using Library;
using Media;
using UnityEngine;
using VideoPlaying;
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
		private readonly OptionsSettings _optionsSettings;
		private readonly ProjectionController _projectionController;
		private GameObject _currentScreen;

#if UNITY_STANDALONE || (UNITY_EDITOR && !UNITY_ANDROID)
		public ScreensManager(ICommonFactory factory, MainConfig mainConfig, Transform canvasTransform,
			Action<MediaContent> playAction, ContourEditorController contourEditorController,
			MediaController mediaController, OptionsSettings optionsSettings, ProjectionController projectionController)
		{
			_factory = factory;
			_mainConfig = mainConfig;
			_canvasTransform = canvasTransform;
			_playAction = playAction;
			_contourEditorController = contourEditorController;
			_mediaController = mediaController;
			_optionsSettings = optionsSettings;
			_projectionController = projectionController;
		}

#elif UNITY_ANDROID
		public ScreensManager(ICommonFactory factory, MainConfig mainConfig, Transform canvasTransform)
		{
			_factory = factory;
			_mainConfig = mainConfig;
			_canvasTransform = canvasTransform;

			OpenWindow(ScreenType.MainMenu);
		}
#endif

		public void DestroyCurrentScreen() => Object.Destroy(_currentScreen);

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
				case ScreenType.ExitConfirmationPopup:
					InitExitPopUp(screen);
					break;
				case ScreenType.LoginToServerPopup:
					InitServerPopUp(screen);
					break;
				case ScreenType.SettingsScreen:
					InitSettingsScreen(screen);
					break;
				case ScreenType.PasswordPopup:
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private static bool IsPopup(ScreenType type) => (byte)type > QTS_POPUP_ID;

		private void InitMainMenu(GameObject screen)
		{
#if UNITY_STANDALONE
			var mainMenu = screen.GetComponent<MainMenu>();
			mainMenu.Init(_mediaController, PlayVideo,
				() => OpenPasswordPopUp(() => OpenWindow(ScreenType.AdminMenu)), 
				Application.Quit, _mainConfig.MediaItemPrefab, _factory, _optionsSettings);

			_projectionController.InitOutputType();
#elif UNITY_ANDROID
			var mainMenu = screen.GetComponent<MainMenuAndroid>();
			mainMenu.Init(_mainConfig.MediaItemPrefab, _factory, 
				() => OpenWindow(ScreenType.LoginToServerPopup));
#endif
		}

		public void ReloadMediaItems(MediaContent[] media, ICommonFactory factory, GameObject mediaPrefab, Action<MediaContent> playVideoAction)
		{
			return;
			//var mainMenu = _currentScreen.GetComponent<MainMenu>();
			//mainMenu.ClearMediaItems();
			//mainMenu.InitMediaItems(media, factory, mediaPrefab, PlayVideo);
		}

		public void SetMediaInteractable()
		{
			//var mainMenu = _currentScreen.GetComponent<MainMenu>();
		}

		private void InitAdminMenu(GameObject screen)
		{
			var adminMenu = screen.GetComponent<AdminMenu>();
			adminMenu.Init(OpenPatternsEditor,
				() => OpenPasswordPopUp(() => OpenWindow(ScreenType.SettingsScreen)),
				() => OpenWindow(ScreenType.MainMenu));
		}

		private void InitSettingsScreen(GameObject screen)
		{
			var libraryOptions = screen.GetComponent<SettingScreen>();
			libraryOptions.Init(_factory, () => OpenWindow(ScreenType.AdminMenu), _optionsSettings);
		}

		private void InitExitPopUp(GameObject screen)
		{
			var exitPopUp = screen.GetComponent<ExitPopUp>();
			exitPopUp.Init(Application.Quit);
		}

		private void InitServerPopUp(GameObject screen)
		{
			var serverPopup = screen.GetComponent<ServerLoginPopup>();
			serverPopup.Init();
		}

		private void OpenPasswordPopUp(Action onContinue)
		{
			var screen = ShowScreen(ScreenType.PasswordPopup);
			var passwordPopUp = screen.GetComponent<PasswordPopUp>();

			if (LoginHelper.IsLoggedIn())
			{
				onContinue?.Invoke();
				Object.Destroy(screen);
			}

			void CancelAction() => OpenWindow(ScreenType.MainMenu);

			passwordPopUp.Init((password) =>
			{
				if (password != LoginHelper.GetPassword())
					return;
				
				onContinue?.Invoke();
				Object.Destroy(screen);
			}, CancelAction);
		}

		public void PlayVideo(MediaContent content)
		{
			if(_projectionController.OutputType < OutputType.Secondary)
				DestroyCurrentScreen();

			_playAction?.Invoke(content);
		}

		private void OpenPatternsEditor()
		{
			_contourEditorController?.Show(() => OpenWindow(ScreenType.AdminMenu));

			Object.Destroy(_currentScreen);
		}
	}
}