using System.Linq;
using UnityEngine;

namespace Configs
{
	[CreateAssetMenu(fileName = "ThumbnailsConfig", menuName = "Configs/Create thumbnails config")]
	public class ThumbnailsConfig : ScriptableObject
	{
		public Sprite[] Thumbnails;

		public Sprite GetThumbnail(string mediaName) =>
			Thumbnails.FirstOrDefault(thumb => thumb.name.Contains(mediaName));
	}
}
