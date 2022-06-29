using Configs;
using Core;
using Screens;
using UnityEngine;

namespace Common
{
	public class Bootstrapper : MonoBehaviour
	{
		[SerializeField] private ScreensConfig _screensConfig;
		[SerializeField] private Transform _canvasTransform;

		private ICommonFactory _factory;
		private ScreensManager _screensManager;
		
		private void Awake()
		{
			_factory = new CommonFactory();

			_screensManager = new ScreensManager(_factory, _screensConfig, _canvasTransform);

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
