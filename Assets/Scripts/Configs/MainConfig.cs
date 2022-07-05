using UnityEngine;

namespace Configs
{
	[CreateAssetMenu(fileName = "MainConfig", menuName = "Configs/Application config")]
	public class MainConfig : ScriptableObject
	{
		public GameObject ProjectionSetup;
		public MediaConfig MediaConfig;
		public ScreensConfig ScreensConfig;
	}
}
