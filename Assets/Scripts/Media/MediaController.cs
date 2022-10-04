using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
		private const int CheckMediaDelayMs = 8000;

		public event Action OnDownloadCompleted, OnMediaChanged;

		private const string QTS_URL =
			"https://8eba-37-73-91-76.eu.ngrok.io/api/media?token=30b1ebfd3225b7b0454854ad59135df86d78372d70bb0a553d1e417c3f7bb3df";

		private const string QTS_IMAGE_EXTENSION = ".jpg";
		private const string QTS_VIDEO_EXTENSION = ".mp4";
		private static readonly string[] AllowedExtensions = { QTS_IMAGE_EXTENSION, QTS_VIDEO_EXTENSION };
		private readonly ProjectionController _projectionController;
		private readonly Action _showMediaRemovedPopup;
		private bool _isMediaCheckRunning;

		public bool IsDownloading { get; private set; } = true;
		public MediaContent[] MediaFiles { get; private set; }

#if UNITY_ANDROID
		public MediaController()
		{
		}
#elif UNITY_STANDALONE
		public MediaController(ProjectionController projectionController, Action showMediaRemovedPopup)
		{
			_projectionController = projectionController;
			_showMediaRemovedPopup = showMediaRemovedPopup;

			LoadMediaFromLocalStorage();
		}
#endif

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

		public async void LoadMediaFromServer()
		{
			var isConnectionAvailable = NetworkHelper.IsConnectionAvailable();

			Debug.Log("Connection status: " + isConnectionAvailable);

			if(!isConnectionAvailable)
				return;

			try
			{
				var mediaFiles = GetMediaListFromServer();
				var mediaUrls = new List<string>();
				var thumbnailUrls = new List<string>();

				foreach (var f in mediaFiles)
					if (IsExtensionMatched(f.media))
					{
						mediaUrls.Add(f.media);
						thumbnailUrls.Add(f.thumbnail);
					}

				await ValidateContentAsync(mediaUrls, thumbnailUrls);

				OnDownloadCompleted?.Invoke();
			}
			catch (Exception e)
			{
				Debug.Log(e);
			}
			finally
			{
			}
		}

		private static MediaFile[] GetMediaListFromServer()
		{
			MediaFile[] media = null;

			var request = WebRequest.Create(QTS_URL);
			request.Headers.Add("InstallationId", "AndrewTest");

			try
			{
				var response = request.GetResponse();

				using var reader = new StreamReader(response.GetResponseStream()!);

				var result = reader.ReadToEnd();
				var mediaFilesHelper = new MediaFilesHelper();

				return mediaFilesHelper.GetList(result).MediaFiles;
			}
			catch (Exception e)
			{
				Debug.LogError(e);

				return null;
			}
		}

		private async Task ValidateContentAsync(IReadOnlyCollection<string> mediaUrls,
			IReadOnlyCollection<string> thumbnailUrls)
		{
			await ValidateContent(mediaUrls, Settings.MediaPath);
			await ValidateContent(thumbnailUrls, Settings.ThumbnailsPath);

			Settings.LoadLibrary();

			LoadMediaFromLocalStorage();
			CompleteDownloading();
		}

		private void CompleteDownloading()
		{
			DownloadingCallback();

			_isMediaCheckRunning = true;

			CheckMediaUpdates();
		}

		private async void CheckMediaUpdates()
		{
			while (_isMediaCheckRunning)
			{
				await Task.Delay(CheckMediaDelayMs);
				Debug.Log("updates check");
				var mediaListFromServer = GetMediaListFromServer();
				var mediaUrls = new List<string>();
				var thumbnailUrls = new List<string>();

				foreach (var f in mediaListFromServer)
					if (IsExtensionMatched(f.media))
					{
						mediaUrls.Add(f.media);
						thumbnailUrls.Add(f.thumbnail);
					}

				var mediaWasRemoved = false;

				if (mediaListFromServer.Length == MediaFiles.Length)
				{
					Debug.Log("no updates");

					continue;
				}

				if(mediaListFromServer.Length < MediaFiles.Length)
					mediaWasRemoved = true;

				IsDownloading = true;

				await ValidateContent(thumbnailUrls, Settings.ThumbnailsPath);
				await ValidateContent(mediaUrls, Settings.MediaPath);

				Settings.LoadLibrary();

				LoadMediaFromLocalStorage();

				if (mediaWasRemoved && !string.IsNullOrEmpty(_projectionController?.CurrentPlayingMediaName))
				{
					var isRemovedMediaPlayed =
						MediaFiles.All(t => t.Name != _projectionController.CurrentPlayingMediaName);

					if (isRemovedMediaPlayed)
					{
						_projectionController?.StopAndHidePlayer();

						_showMediaRemovedPopup?.Invoke();
					}
				}

				OnMediaChanged?.Invoke();

				DownloadingCallback();
			}
		}

		private void DownloadingCallback()
		{
			OnDownloadCompleted?.Invoke();

			IsDownloading = false;
		}

		private async Task ValidateContent(IReadOnlyCollection<string> urls, string path)
		{
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			var mediaToDownload = GetFilesForDownload(urls, path);
			CheckFilesForDelete(urls, path);

			await DownloadAndSaveFiles(mediaToDownload, path);
		}

		private List<string> GetFilesForDownload(IReadOnlyCollection<string> urls, string path)
		{
			var mediaToDownload = new List<string>(urls.Count);

			foreach (var url in urls)
			{
				var fileNameInWeb = Path.GetFileName(url).Trim();
				var fileName = fileNameInWeb.Split(Constants.Underscore).Last();
				var downloadPath = Path.Combine(path, fileName);
				
				if(File.Exists(downloadPath))
					continue;

				mediaToDownload.Add(url);
			}

			return mediaToDownload;
		}

		private void CheckFilesForDelete(IReadOnlyCollection<string> urls, string path)
		{
			var files = Directory.GetFiles(path);

			foreach (var file in files)
			{
				var isNeedDeleteFile = true;
				
				foreach (var url in urls)
				{
					var fileNameInWeb = Path.GetFileName(url).Trim();
					fileNameInWeb = fileNameInWeb.Split(Constants.Underscore).Last();
					var downloadPath = Path.Combine(path, fileNameInWeb);
					
					if (downloadPath == file)
					{
						isNeedDeleteFile = false;
						break;
					}
				}
				
				if (isNeedDeleteFile)
					File.Delete(file);
			}
		}

		private async Task DownloadAndSaveFiles(IEnumerable<string> urls, string path)
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
					var fileNameInWeb = Path.GetFileName(url);
					var fileName = fileNameInWeb.Split(Constants.Underscore).Last();
					var savePath = Path.Combine(path, fileName);

					await File.WriteAllBytesAsync(savePath, www.downloadHandler.data);
				}
			}
		}
		
		private static bool IsExtensionMatched(string path) =>
			path.Contains(AllowedExtensions[0]) || path.Contains(AllowedExtensions[1]);

		public void Clear()
		{
			_isMediaCheckRunning = false;
		}
	}
}
