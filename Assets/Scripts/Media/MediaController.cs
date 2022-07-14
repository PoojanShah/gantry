using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Configs;
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

		private string GetMediaPath()
		{
			return Directory.GetParent(Application.dataPath) + "/Media/";
		}

		public MediaController(MediaConfig config)
		{
			LoadMediaInfoFromLocalStorage(config);
		}

		private void LoadMediaInfoFromLocalStorage(MediaConfig mediaConfig)
		{
			var files = Directory.GetFiles(UrlUnity);

			mediaConfig.InitMediaContent(files);
		}

		private async Task<byte[]> LoadVideo(string path)
		{
			var www = UnityWebRequest.Get(path);
			www.SendWebRequest();

			while (!www.isDone)
				await Task.Delay(1);

			return www.downloadHandler.data;
		}
	}
}
