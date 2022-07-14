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
			_projectionController = new ProjectionController(_factory, _mainConfig.ProjectionSetup,
				_mainConfig.MediaConfig, () => _screensManager.OpenWindow(ScreenType.MainMenu));
			_contourEditorController = new ContourEditorController(_projectionController.GetProjection());
			_screensManager = new ScreensManager(_factory, _mainConfig, _canvasTransform, _projectionController.Play, _contourEditorController.Show);
			_mediaController = new MediaController(_mainConfig.MediaConfig);

			InitSettings();
		}

		private static void InitSettings()
		{
			Settings.appDir = Application.persistentDataPath;
			Settings.Load();
			Settings.LoadLibraryAndCategories();
			Settings.initialScreenWidth = Screen.currentResolution.width;
		}
	}
}
