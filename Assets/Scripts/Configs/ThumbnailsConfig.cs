using UnityEngine;

namespace Configs
{
	[CreateAssetMenu(fileName = "ThumbnailsConfig", menuName = "Configs/Create thumbnails config")]
	public class ThumbnailsConfig : ScriptableObject
	{
		public Sprite[] Thumbnails;
	}
}
