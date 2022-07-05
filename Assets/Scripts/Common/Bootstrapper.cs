using Configs;
using ContourEditorTool;
using Core;
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
		
		private void Awake()
		{
			_factory = new CommonFactory();
			_projectionController = new ProjectionController(_factory, _mainConfig.ProjectionSetup,
				_mainConfig.MediaConfig, () => _screensManager.OpenWindow(ScreenType.MainMenu));
			_contourEditorController = new ContourEditorController(_projectionController.GetProjection());
			_screensManager = new ScreensManager(_factory, _mainConfig, _canvasTransform, _projectionController.Play, _contourEditorController.Show);

			InitSettings();
		}

		private static void InitSettings()
		{
#if UNITY_EDITOR
			//Nate: Added this so saving and video playback works on my local machine. 

			// Settings.libraryDir = Settings.GetVideoFolder();
			Settings.appDir = Application.persistentDataPath;
			Settings.noPersistFile = Settings.appDir + SRSUtilities.slashChar + "halt.motions";
#endif
			Settings.Load();
			Settings.LoadLibraryAndCategories();
			Settings.initialScreenWidth = Screen.currentResolution.width; //Screen.width;
			//Settings.dongleKeys=new string[]{"DE2E19C984E2925D","D85D6EA1539B7493"};
		}
	}
}
