using System;
using System.IO;
using System.Linq;
using Configs;
using ContourEditorTool;
using Core;
using Media;
using Network;
using Screens;
using UnityEngine;
using VideoPlaying;

namespace Common
{
	public class Bootstrapper : MonoBehaviour
	{
		[SerializeField] private MainConfig _mainConfig;
		[SerializeField] private Transform _canvasTransform;

		private ICommonFactory _factory;
		private ScreensManager _screensManager;
		private ProjectionController _projectionController;
		private ContourEditorController _contourEditorController;
		private MediaController _mediaController;
		private NetworkController _networkController;
		private OptionsSettings _settings;

		private void Awake()
		{
			CameraHelper.Init();

			InitSettings();

			_factory = new CommonFactory();
			_mediaController = new MediaController();
			
			_settings = new OptionsSettings();

			_projectionController = new ProjectionController(_factory, _mainConfig.ProjectionSetup,
				() => _screensManager.OpenWindow(ScreenType.MainMenu), _settings);
			_contourEditorController = new ContourEditorController(_projectionController, _factory,
				_mainConfig.ContourEditorUiPrefab);
#if UNITY_STANDALONE || (UNITY_EDITOR && !UNITY_ANDROID)
			_screensManager = new ScreensManager(_factory, _mainConfig, _canvasTransform, _projectionController.Play,
				_contourEditorController, _mediaController, _settings, _projectionController);
#endif

			InitNetwork();

			if(!WasCrashed(_screensManager.PlayVideo))
				_screensManager.OpenWindow(ScreenType.MainMenu);
			
			_mediaController.LoadMediaFromServer();
		}

		private bool WasCrashed(Action<MediaContent> playVideoAction)
		{
			MediaContent FindAndRestoreMedia(string path)
			{
				var mediaNamePrimary = Path.GetFileName(path);

				if (string.IsNullOrEmpty(mediaNamePrimary))
					return null;

				var media = _mediaController.MediaFiles.FirstOrDefault(m => m.Name == mediaNamePrimary);

				return media;
			}

			if (!PlayerPrefs.HasKey(Constants.LastPlayedSecondaryMediaHash))
				return false;

			var content = FindAndRestoreMedia(PlayerPrefs.GetString(Constants.LastPlayedSecondaryMediaHash));

			if(content != null)
				playVideoAction?.Invoke(content);
			else
				_screensManager.OpenWindow(ScreenType.MainMenu);

			if (!PlayerPrefs.HasKey(Constants.LastPlayedPrimaryMediaHash))
			{
				playVideoAction?.Invoke(content);

				return false;
			}

			content = FindAndRestoreMedia(PlayerPrefs.GetString(Constants.LastPlayedPrimaryMediaHash));

			if (content != null)
				playVideoAction?.Invoke(content);
			else
				_screensManager.OpenWindow(ScreenType.MainMenu);

			return true;
		}

		private void Update() => HandleRemoteMessages();

		private void HandleRemoteMessages()
		{
			if (LocalNetworkServer.ReceivedId < 0)
				return;

			_screensManager.DestroyCurrentScreen();

			_projectionController.Play(_mediaController.MediaFiles[LocalNetworkServer.ReceivedId]);

			LocalNetworkServer.ReceivedId = -1;
		}

		private void InitNetwork()
		{
#if UNITY_STANDALONE_WIN // not working on MAC
			_networkController = new NetworkController(_mediaController, _settings);

			//NetworkHelper.GetMessage();
#endif
		}

		private void OnDestroy()
		{
			_networkController.Clear();
			_mediaController.Clear();
		}

		private static void InitSettings()
		{
			Settings.LoadLibrary();
			Settings.InitialScreenWidth = Screen.currentResolution.width;
		}
	}
}
