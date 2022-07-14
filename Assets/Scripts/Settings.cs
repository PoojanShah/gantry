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
	private const string QTS_CUE_CORE_IP_HASH = "CueCoreIP";
	private const string QTS_CUE_CORE_PORT_HASH = "CueCorePort";
	private const string QTS_SERVER_CHECK_HASH = "ServerCheck";
	private const string QTS_USE_CUE_CORE_HASH = "UseCueCore";
	private const string QTS_ROTATION_HASH = "Rotation";
	private const string QTS_VOLUME_HASH = "Volume";
	private const string QTS_VERSION_HASH = "Version";

	public static int initialScreenWidth;

	public static string[] mediaLibrary;

	public static Dictionary<string, string> videoColor = new Dictionary<string, string>();
	public static string cuecoreIP = "192.168.1.10";
	public static int cuecorePort = 7000;


	public static bool serverCheck = true, useCueCore = false; //Whether or not to shut down if we don't hear from the master server. We only turn off for demonstration environments.

	public static bool sound = true;
	public static bool _persist = true;
	public static bool rotation = true;
	public static float volume = 1.0f;
	public static string dataPath = Directory.GetParent(Application.dataPath).ToString();


	public static string colorsConfigPath = dataPath + "/moviecolors.cfg";

	public enum MonitorMode
	{
		Single = 0,
		Dual
	};

	public static float originalScaleX = (float)Screen.width / (float)Screen.height;

	public static int version
	{
		get => PlayerPrefs.HasKey(QTS_VERSION_HASH) ? PlayerPrefs.GetInt(QTS_VERSION_HASH) : 1;
		set => PlayerPrefs.SetInt(QTS_VERSION_HASH, value);
	}

	public static MonitorMode monitorMode { get; set; } = MonitorMode.Single;

	public static void ShowCursor(bool isShow = true)
	{
		Cursor.lockState = isShow ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = isShow;
	}

	public static void Save()
	{
		PlayerPrefs.SetString(QTS_CUE_CORE_IP_HASH, cuecoreIP);
		PlayerPrefs.SetInt(QTS_CUE_CORE_PORT_HASH, cuecorePort);
		PlayerPrefs.SetInt(QTS_SERVER_CHECK_HASH, Convert.ToInt32(serverCheck));
		PlayerPrefs.SetInt(QTS_USE_CUE_CORE_HASH, Convert.ToInt32(useCueCore));
		PlayerPrefs.SetInt(QTS_ROTATION_HASH, Convert.ToInt32(rotation));
		PlayerPrefs.SetFloat(QTS_VOLUME_HASH, volume);
	}

	public static void Load()
	{
		cuecoreIP = PlayerPrefs.GetString(QTS_CUE_CORE_IP_HASH, cuecoreIP);
		cuecorePort = PlayerPrefs.GetInt(QTS_CUE_CORE_PORT_HASH, cuecorePort);
		rotation = Convert.ToBoolean(PlayerPrefs.GetInt(QTS_ROTATION_HASH, Convert.ToInt32(rotation)));
		serverCheck = Convert.ToBoolean(PlayerPrefs.GetInt(QTS_SERVER_CHECK_HASH, Convert.ToInt32(serverCheck)));
		useCueCore = Convert.ToBoolean(PlayerPrefs.GetInt(QTS_USE_CUE_CORE_HASH, Convert.ToInt32(useCueCore)));

		volume = PlayerPrefs.GetFloat(QTS_VOLUME_HASH, volume);
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
		var files = Directory.GetFiles(MediaController.LibraryPath, Constants.AllFilesPattern);
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
