using Core;
using UnityEngine;

namespace Configs
{
	[CreateAssetMenu(fileName = "ThumbnailsConfig", menuName = "Configs/Create thumbnails config")]
	public class ThumbnailsConfig : ScriptableObject
	{
		public Sprite[] Thumbnails;

		public Sprite GetThumbnail(string mediaName)
		{
			return null;
			foreach (var t in Thumbnails)
			{
				var thumbSplitName = t.name.Split(Constants.Hyphen)[1];

				if(mediaName == thumbSplitName)
					return t;
			}

			return null;
		}
	}
}
