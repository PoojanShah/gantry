using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Media
{
	public class MediaController
	{
		public event Action OnMediaFileDownloaded, OnDownloadCompleted;
		private const string QTS_URL = "http://192.168.1.114/GantryMedia/";
		private const string QTS_REGEX_PATTERN = "<a href=\".*\">(?<name>.*)</a>";
		private const string QTS_IMAGE_EXTENSION = ".jpg";
		private const string QTS_VIDEO_EXTENSION = ".mp4";
		private static readonly string[] AllowedExtensions = { QTS_IMAGE_EXTENSION, QTS_VIDEO_EXTENSION };

		public bool IsDownloading { get; private set; } = true;
		public MediaContent[] MediaFiles { get; private set; }

		public MediaController() => LoadMediaFromLocalStorage();

		public void InitMediaContent(string[] paths)
		{
			MediaFiles = new MediaContent[paths.Length];

			for (var i = 0; i < paths.Length; i++)
			{
				MediaFiles[i] = new MediaContent
				{
					Path = paths[i],
					Name = Path.GetFileNameWithoutExtension(paths[i]),
					IsVideo = Path.GetExtension(paths[i]) == QTS_VIDEO_EXTENSION
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
			if(!Directory.Exists(Settings.MediaPath))
				return;

			var files = Directory.GetFiles(Settings.MediaPath);

			InitMediaContent(files);
		}

		public void LoadMediaFromServer()
		{
			var request = WebRequest.Create(QTS_URL);
			try
			{
				var response = request.GetResponse();
				var regex = new Regex(QTS_REGEX_PATTERN);
				const string regexHash = "name";

				using var reader = new StreamReader(response.GetResponseStream()!);

				var result = reader.ReadToEnd();
				var matches = regex.Matches(result);
				var mediaUrls = new List<string>(matches.Count);

				foreach (Match match in matches)
				{
					var path = match.ToString();

					if (IsExtensionMatched(path))
						mediaUrls.Add(QTS_URL + match.Groups[regexHash].ToString().Trim());
				}

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
