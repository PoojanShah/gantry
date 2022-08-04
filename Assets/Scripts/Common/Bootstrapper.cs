using Configs;
using ContourEditorTool;
using Core;
using Media;
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
		
		private void Awake()
		{
			CameraHelper.Init();

			_factory = new CommonFactory();
			_mediaController = new MediaController();
			_projectionController = new ProjectionController(_factory, _mainConfig.ProjectionSetup,
				() => _screensManager.OpenWindow(ScreenType.MainMenu));
			_contourEditorController = new ContourEditorController(_projectionController, _factory, _mainConfig.ContourEditorUiPrefab);
			_screensManager = new ScreensManager(_factory, _mainConfig, _canvasTransform, _projectionController.Play,
				_contourEditorController, _mediaController);

			_mediaController.OnMediaFileDownloaded += ReloadMediaFile;
			_mediaController.OnDownloadCompleted += ActivateLoadingItems;

			_mediaController.LoadMediaFromServer();

			InitSettings();
		}

		private void OnDestroy()
		{
			_mediaController.OnMediaFileDownloaded -= ReloadMediaFile;
			_mediaController.OnDownloadCompleted -= ActivateLoadingItems;
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
