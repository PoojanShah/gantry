using Configs;
using Core;
using Network;
using Screens;
using UnityEngine;

namespace Common
{
	public class BootstrapperAndroid : MonoBehaviour
	{
		[SerializeField] private MainConfig _mainConfig;
		[SerializeField] private Transform _canvasTransform;

		private ICommonFactory _factory;
		private ScreensManager _screensManager;

		private void Awake()
		{
			CameraHelper.Init();

			_factory = new CommonFactory();

			_screensManager = new ScreensManager(_factory, _mainConfig, _canvasTransform, null,
				null, null);

			InitNetwork();
		}

		private void InitNetwork()
		{
		}
	}
}
