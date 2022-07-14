using System.IO;
using Media;
using UnityEngine;

namespace Configs
{
	[CreateAssetMenu(fileName = "MediaConfig", menuName = "Configs/Media config")]
	public class MediaConfig : ScriptableObject
	{
		public GameObject MediaItemPrefab;

		public MediaContent[] MediaFiles { get; private set; }

		public void InitMediaContent(string[] paths)
		{
			MediaFiles = new MediaContent[paths.Length];

			for (var i = 0; i < paths.Length; i++)
			{
				MediaFiles[i] = new MediaContent
				{
					Path = paths[i], 
					Name = Path.GetFileNameWithoutExtension(paths[i]),
					IsVideo = Path.GetExtension(paths[i]) == ".mp4"
				};
			}
		}
	}
}
