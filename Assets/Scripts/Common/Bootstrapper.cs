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
			_mediaController = new MediaController(_mainConfig.ThumbnailsConfig);
			
			_settings = new OptionsSettings();

			_projectionController = new ProjectionController(_factory, _mainConfig.ProjectionSetup,
				() => _screensManager.OpenWindow(ScreenType.MainMenu), _settings);
			_contourEditorController = new ContourEditorController(_projectionController, _factory,
				_mainConfig.ContourEditorUiPrefab);
#if UNITY_STANDALONE || (UNITY_EDITOR && !UNITY_ANDROID)
			_screensManager = new ScreensManager(_factory, _mainConfig, _canvasTransform, _projectionController.Play,
				_contourEditorController, _mediaController, _settings);
#endif

			_mediaController.OnMediaFileDownloaded += ReloadMediaFile;
			_mediaController.OnDownloadCompleted += ActivateLoadingItems;

			//_mediaController.LoadMediaFromServer();

			InitNetwork();

			if(!WasCrashed(_screensManager.PlayVideo))
				_screensManager.OpenWindow(ScreenType.MainMenu);
		}

		private bool WasCrashed(Action<MediaContent> playVideoAction)
		{
			if (!PlayerPrefs.HasKey(Constants.LastPlayedMediaHash))
				return false;

			var mediaName = Path.GetFileName(PlayerPrefs.GetString(Constants.LastPlayedMediaHash));

			if (string.IsNullOrEmpty(mediaName))
				return false;

			var media = _mediaController.MediaFiles.FirstOrDefault(m => m.Name == mediaName);

			if (media == null)
				return false;

			playVideoAction?.Invoke(media);

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
#if UNITY_STANDALONE_WIN
			_networkController = new NetworkController(_mediaController, _settings);
#endif
		}

		private void OnDestroy()
		{
			_mediaController.OnMediaFileDownloaded -= ReloadMediaFile;
			_mediaController.OnDownloadCompleted -= ActivateLoadingItems;

			_networkController.Clear();
		}

		private void ActivateLoadingItems() => _screensManager.SetMediaInteractable();

		private void ReloadMediaFile()
		{
			Settings.LoadLibrary();

			_screensManager.ReloadMediaItems(_mediaController.MediaFiles, _factory, _mainConfig.MediaItemPrefab,
				_projectionController.Play);
		}

		private static void InitSettings()
		{
			Settings.LoadLibrary();
			Settings.InitialScreenWidth = Screen.currentResolution.width;
		}
	}
}
