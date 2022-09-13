using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Configs;
using Core;
using Network;
using UnityEngine;
using UnityEngine.Networking;
using VideoPlaying;

namespace Media
{
	public class MediaController
	{
		public event Action OnMediaFileDownloaded, OnDownloadCompleted;

		private const string QTS_URL =
			"https://api.comfort-health.net/api/videos?token=30b1ebfd3225b7b0454854ad59135df86d78372d70bb0a553d1e417c3f7bb3df";

		private const string QTS_IMAGE_EXTENSION = ".jpg";
		private const string QTS_VIDEO_EXTENSION = ".mp4";
		
		private static readonly string[] AllowedExtensions = { QTS_IMAGE_EXTENSION, QTS_VIDEO_EXTENSION };

		public bool IsDownloading { get; private set; } = true;
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
					Name = Path.GetFileName(paths[i]),
					IsVideo = Path.GetExtension(paths[i]) == QTS_VIDEO_EXTENSION,
					Id = i
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

		public static Texture2D LoadThumbnail(string mediaName)
		{
			var noExtensionName = Path.GetFileNameWithoutExtension(mediaName);
			var realPath = Path.Combine(Settings.ThumbnailsPath,
				Constants.ThumbnailsPrefix + noExtensionName + Constants.ExtensionPng);

			return !File.Exists(realPath) ? null : LoadImageFromFile(realPath);
		}

		private void LoadMediaFromLocalStorage()
		{
			if(!Directory.Exists(Settings.MediaPath))
				return;

			var files = Directory.GetFiles(Settings.MediaPath);

			InitMediaContent(files);
		}

		public void LoadMediaFromServer()
		{
			Debug.Log("Connection status: " + NetworkHelper.IsConnectionAvailable());
			
			var request = WebRequest.Create(QTS_URL);
			try
			{
				var response = request.GetResponse();

				using var reader = new StreamReader(response.GetResponseStream()!);

				var result = reader.ReadToEnd();

				var mediaFilesHelper = new MediaFilesHelper();
				var mediaFiles = mediaFilesHelper.GetList(result).MediaFiles;
				var mediaUrls = new List<string>();

				foreach (var f in mediaFiles)
					if (IsExtensionMatched(f.media))
						mediaUrls.Add(f.media);

				CheckFilesForDownload(mediaUrls);
			}
			catch (Exception e)
			{
				Debug.Log(e);
			}
			finally
			{
				CompleteDownloading();
			}
		}

		private void CompleteDownloading()
		{
			OnDownloadCompleted?.Invoke();

			IsDownloading = false;
		}

		private void CheckFilesForDownload(IReadOnlyCollection<string> urls)
		{
			if (!Directory.Exists(Settings.MediaPath))
				Directory.CreateDirectory(Settings.MediaPath);

			var mediaToDownload = new List<string>(urls.Count);

			foreach (var url in urls)
			{
				var fileName = Path.GetFileName(url).Trim();
				var downloadPath = Path.Combine(Settings.MediaPath, fileName);
				
				if(File.Exists(downloadPath))
					continue;

				mediaToDownload.Add(url);
			}

			DownloadAndSaveFiles(mediaToDownload);
		}

		private async void DownloadAndSaveFiles(IEnumerable<string> urls)
		{
			foreach (var url in urls)
			{
				using var www = UnityWebRequest.Get(url);
				www.SendWebRequest();

				while (!www.isDone)
					await Task.Delay(1);

				if ((int) www.result > 1)
					Debug.Log(www.error);
				else
				{
					var savePath = Path.Combine(Settings.MediaPath, Path.GetFileName(url));

					await File.WriteAllBytesAsync(savePath, www.downloadHandler.data);

					LoadMediaFromLocalStorage();

					OnMediaFileDownloaded?.Invoke();
				}
			}

			CompleteDownloading();
		}
		
		private static bool IsExtensionMatched(string path) =>
			path.Contains(AllowedExtensions[0]) || path.Contains(AllowedExtensions[1]);
	}
}
