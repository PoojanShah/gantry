using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Core;
using Media;

public static class Settings
{
	public const int ScreenW = 1024, ScreenH = 768, slideInterval = 15;

	public static int initialScreenWidth;

	public static string[] mediaLibrary;

	public static Dictionary<string, string> videoColor = new Dictionary<string, string>();

	public static bool rotation = true;
	
	public static string buildPath = Directory.GetParent(Application.dataPath).ToString();
	public static string colorsConfigPath = buildPath + "/moviecolors.cfg";

#if UNITY_EDITOR
	public static readonly string MediaPath = buildPath + "/Build/GantryMedia/";
	public static readonly string DownloadedMediaPath = buildPath + "/Build/DownloadedGantryMedia/";
	public static string gantryPatternsPath = buildPath + "/Build/meshes/";
#elif UNITY_STANDALONE_WIN
	public static readonly string MediaPath = buildPath + "/GantryMedia/";
	public static readonly string DownloadedMediaPath = buildPath + "/DownloadedGantryMedia/";
	public static string gantryPatternsPath = buildPath + "/meshes/";
#endif

	public enum MonitorMode
	{
		Single = 0,
		Dual
	};

	public static float originalScaleX = (float)Screen.width / (float)Screen.height;

	public static MonitorMode monitorMode { get; set; } = MonitorMode.Single;

	public static void ShowCursor(bool isShow = true)
	{
		Cursor.lockState = isShow ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = isShow;
	}

	public static void LoadLibrary()
	{
		LoadMediaLibrary();

		if (File.Exists(colorsConfigPath))
			videoColor = LoadMovieColors(colorsConfigPath);

		for (var i = 0; i < mediaLibrary.Length; i++)
			if (!videoColor.ContainsKey(mediaLibrary[i]))
				videoColor[mediaLibrary[i]] =
					Constants.colorDefaults[i % Constants.colorDefaults.Length]
						.Key;
	}

	private static void LoadMediaLibrary()
	{
		var files = Directory.GetFiles(MediaPath, Constants.AllFilesPattern);
		var libraryTemp = new List<string>(files.Length);

		libraryTemp.AddRange(from file in files
			where !file.EndsWith(Constants.ExtensionMeta)
			select Path.GetFileName(file));

		mediaLibrary = libraryTemp.ToArray();
	}

	private static Dictionary<string, string> LoadMovieColors(string movieColorFile)
	{
		var movieColors = new Dictionary<string, string>();

		try
		{
			const string split = ";";
			var reader = new StreamReader(movieColorFile);

			while (reader.ReadLine() is { } line)
			{
				var movieColor = line.Trim().Split(split[0]);

				if (movieColor.Length != 2)
					continue;

				movieColors[movieColor[0]] = movieColor[1];
			}

			reader.Close();
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}

		return movieColors;
	}

}
