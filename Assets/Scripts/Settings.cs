using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Core;

public static class Settings
{
	public const int ScreenWidth = 1920, ScreenHeight = 1080;

	public static int InitialScreenWidth;
	public static string[] MediaLibrary;
	public static Dictionary<string, string> VideoColors = new();
	public static bool IsRotation = true;
	public static string BuildPath = Directory.GetParent(Application.dataPath).ToString();
	public static string ColorsConfigPath = BuildPath + "/moviecolors.cfg";

#if UNITY_EDITOR
	public static readonly string MediaPath = BuildPath + "/Build/DownloadedGantryMedia/";
	public static string GantryPatternsPath = BuildPath + "/Build/meshes/";
#elif UNITY_STANDALONE_WIN
	public static readonly string MediaPath = BuildPath + "/DownloadedGantryMedia/";
	public static string GantryPatternsPath = BuildPath + "/meshes/";
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

		if (File.Exists(ColorsConfigPath))
			VideoColors = LoadMovieColors(ColorsConfigPath);

		if(MediaLibrary == null || MediaLibrary.Length == 0)
			return;

		for (var i = 0; i < MediaLibrary.Length; i++)
			if (!VideoColors.ContainsKey(MediaLibrary[i]))
				VideoColors[MediaLibrary[i]] =
					Constants.colorDefaults[i % Constants.colorDefaults.Length]
						.Key;
	}

	private static void LoadMediaLibrary()
	{
		if(!Directory.Exists(MediaPath))
			return;

		var files = Directory.GetFiles(MediaPath, Constants.AllFilesPattern);
		var libraryTemp = new List<string>(files.Length);

		libraryTemp.AddRange(from file in files
			where !file.EndsWith(Constants.ExtensionMeta)
			select Path.GetFileName(file));

		MediaLibrary = libraryTemp.ToArray();
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