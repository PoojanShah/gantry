using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace Media
{
	public class MediaController
	{
#if UNITY_EDITOR
		public static readonly string LibraryPath = Settings.dataPath + "/Build/GantryMedia/";
#elif UNITY_STANDALONE_WIN
		public static readonly string LibraryPath = Settings.dataPath + "/GantryMedia/";
#endif

		public MediaContent[] MediaFiles { get; private set; }

		public MediaController()
		{
			LoadMediaFromLocalStorage();
		}

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

		public static Texture2D LoadImageFromFile(string path)
		{
			var bytes = File.ReadAllBytes(path);

			var texture = new Texture2D(2, 2);
			texture.LoadImage(bytes);

			return texture;
		}

		private void LoadMediaFromLocalStorage()
		{
			var files = Directory.GetFiles(LibraryPath);

			InitMediaContent(files);
		}
	}
}
