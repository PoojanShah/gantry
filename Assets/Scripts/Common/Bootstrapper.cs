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
			_contourEditorController = new ContourEditorController(_projectionController.GetProjection());
			_screensManager = new ScreensManager(_factory, _mainConfig, _canvasTransform, _projectionController.Play,
				_contourEditorController.Show, _mediaController);

			_mediaController.OnMediaDownloaded += ReloadMedia;

			InitSettings();
		}

		private void OnDestroy()
		{
			_mediaController.OnMediaDownloaded -= ReloadMedia;
		}

		private void ReloadMedia()
		{
			Settings.LoadLibrary();

			_screensManager.ReloadMediaItems(_mediaController.MediaFiles, _factory, _mainConfig.MediaItemPrefab,
				_projectionController.Play);
		}

		private static void InitSettings()
		{
			Settings.LoadLibrary();
			Settings.initialScreenWidth = Screen.currentResolution.width;
		}
	}
}
