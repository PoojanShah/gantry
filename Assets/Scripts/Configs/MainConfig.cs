using UnityEngine;

namespace Configs
{
	[CreateAssetMenu(fileName = "MainConfig", menuName = "Configs/Application config")]
	public class MainConfig : ScriptableObject
	{
		public GameObject ProjectionSetup;
		public GameObject MediaItemPrefab;
		public ScreensConfig ScreensConfig;
	}
}
