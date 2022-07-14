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
		public const string UrlUnity = "G:/GantryMedia/";

		private const bool IsLocalStorage = true;
		private const string UrlLocal = "http://192.168.1.114/GantryMedia/Videos/";
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

		private void LoadMediaFromLocalStorage()
		{
			var files = Directory.GetFiles(UrlUnity);

			InitMediaContent(files);
		}

		private async Task<byte[]> LoadVideo(string path)
		{
			var www = UnityWebRequest.Get(path);
			www.SendWebRequest();

			while (!www.isDone)
				await Task.Delay(1);

			return www.downloadHandler.data;
		}

		private string GetMediaPath() => Directory.GetParent(Application.dataPath) + "/Media/";
	}
}
