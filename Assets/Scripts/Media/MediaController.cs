using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Media
{
	public class MediaController
	{
		private const string QTS_URL = "http://192.168.1.114/GantryMedia/";
		private const string QTS_REGEX_PATTERN = "<a href=\".*\">(?<name>.*)</a>";
		private static readonly string[] AllowedExtensions = { ".jpg", ".mp4" };

		public MediaContent[] MediaFiles { get; private set; }

		public MediaController()
		{
			LoadMediaFromLocalStorage();

			LoadMediaFromServer();
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
			var files = Directory.GetFiles(Settings.MediaPath);

			InitMediaContent(files);
		}

		private static void LoadMediaFromServer()
		{
			var request = WebRequest.Create(QTS_URL);
			var response = request.GetResponse();
			var regex = new Regex(QTS_REGEX_PATTERN);

			using var reader = new StreamReader(response.GetResponseStream()!);

			var result = reader.ReadToEnd();
			var matches = regex.Matches(result);
			var mediaUrls = new List<string>(matches.Count);

			foreach (Match match in matches)
			{
				var path = match.ToString();

				if (IsExtensionMatched(path))
					mediaUrls.Add(QTS_URL + match.Groups["name"]);
			}
		}

		private static bool IsExtensionMatched(string path) =>
			path.Contains(AllowedExtensions[0]) || path.Contains(AllowedExtensions[1]);
	}
}
