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
		}
	}
}
