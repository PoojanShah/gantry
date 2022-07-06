using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Core;

public static class Settings
{
	public const int ScreenW = 1024, ScreenH = 768, slideInterval = 15;
	private const string QTS_CUE_CORE_IP_HASH = "CueCoreIP";
	private const string QTS_CUE_CORE_PORT_HASH = "CueCorePort";
	private const string QTS_SERVER_CHECK_HASH = "ServerCheck";
	private const string QTS_USE_CUE_CORE_HASH = "UseCueCore";
	private const string QTS_DONGLE_CHECK_HASH = "DongleCheck";
	private const string QTS_DONGLE_KEYS_HASH = "DongleKeys";
	private const string QTS_ROTATION_HASH = "Rotation";
	private const string QTS_VOLUME_HASH = "Volume";
	
	public static int initialScreenWidth;


	public static string heartbeatServerAddress = "www.dtimotions.com/checkin.php",
		clientLogin = "",
		dlServeraddress = "http://www.dtimotions.com",

#if UNITY_EDITOR
		appDir = ".",
		binaryFile = "Motions-Linux.x86_64",
#elif UNITY_STANDALONE_LINUX
        appDir = "/home/motions/app",
        binaryFile = "Motions-Linux.x86_64",
#else
        appDir = ".",
        binaryFile = "Motions-Win64.exe",
#endif
		binaryPath = appDir + SRSUtilities.slashChar + binaryFile,
		newBinaryURL = "http://www.sauerburger.org/dti/" + binaryFile;

	public static string[] mediaLibrary;

	public static float updatePeriod = 60, dongleCheckInterval = 30; //*60*1;//Check the server every minute or hour.
	public static int allowConnectionAttempts = 2; //24;
	public static Dictionary<string, string> videoColor = new Dictionary<string, string>();
	public static string cuecoreIP = "192.168.1.10", dongleChecker = appDir + SRSUtilities.slashChar + "motionsdongle";
	public static int cuecorePort = 7000;


	public static bool
		serverCheck = true,
		dongleCheck = true,
		useCueCore =
			false; //Whether or not to shut down if we don't hear from the master server. We only turn off for demonstration environments.

	public static string[] dongleKeys = new string[] { "DE2E19C984E2925D", "D85D6EA1539B7493" };
	public static bool sound = true;
	public static bool _persist = true;
	public static bool rotation = true;
	public static float volume = 1.0f;
	public static string dataPath = Application.dataPath + "/Videos/";

#if UNITY_EDITOR
	public static string configFile = "motions.cfg",
		commandFile = "cmd",
		libraryDir = Application.dataPath + "/Videos/",
		patientDir = "patient",
		thumbsDir = "Thumbs",
		testBackground = "file://Test.jpg",
		movieColorFile = "moviecolors.cfg";
#elif UNITY_STANDALONE_LINUX
  public static string unlockFile = "/etc/motions/unlocked.cfg",configFile = "/etc/motions/motions.cfg",commandFile =
 "/var/www/run/motions.cmd",libraryDir = "/usr/share/motions/Movies",patientDir =
 "/var/www/html/images/patient",thumbsDir = "/var/www/html/images/thumbs",appDir = "/home/motions/app",testBackground =
 "file://"+appDir+"/Test.jpg",movieColorFile = "/etc/motions/moviecolors.cfg";
#else
  public static string unlockFile = "C:\\motions\\unlocked.cfg",configFile = "C:\\motions\\motions.cfg",commandFile =
 "C:\\motions\\cmd",libraryDir = "Movies",patientDir = "patient",thumbsDir = "Thumbs",testBackground =
 "file:///Test.jpg",movieColorFile = "C:\\motions\\moviecolors.cfg";
#endif

	public enum MonitorMode
	{
		Single = 0,
		Dual
	};

	public enum ScreenMode
	{
		gantry = 0,
		wall,
		gantrywall
	};

	public static float originalScaleX = (float)Screen.width / (float)Screen.height;


	public static int version
	{
		get { return PlayerPrefs.HasKey("Version") ? PlayerPrefs.GetInt("Version") : 1; }
		set { PlayerPrefs.SetInt("Version", value); }
	}

	public static MonitorMode monitorMode { get; set; } = MonitorMode.Single;

	public static ScreenMode screenMode { get; set; }
	public static void NormalizeGUIMatrix() => SRSUtilities.NormalizeGUIMatrix();


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
		PlayerPrefs.SetInt(QTS_DONGLE_CHECK_HASH, Convert.ToInt32(dongleCheck));
		PlayerPrefs.SetString(QTS_DONGLE_KEYS_HASH, string.Join(string.Empty, dongleKeys));
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
		dongleCheck = Convert.ToBoolean(PlayerPrefs.GetInt(QTS_DONGLE_CHECK_HASH, Convert.ToInt32(dongleCheck)));
		dongleKeys = new[]
		{
			PlayerPrefs.GetString(QTS_DONGLE_KEYS_HASH, string.Join(string.Empty, dongleKeys)).Substring(0, 16),
			PlayerPrefs.GetString(QTS_DONGLE_KEYS_HASH, string.Join("", dongleKeys)).Substring(16, 16)
		};
		volume = PlayerPrefs.GetFloat(QTS_VOLUME_HASH, volume);
	}

	public static void LoadLibraryAndCategories()
	{
		LoadMediaLibrary();

		if (File.Exists(movieColorFile))
			videoColor = LoadMovieColors(movieColorFile);

		for (var i = 0; i < mediaLibrary.Length; i++)
			if (!videoColor.ContainsKey(mediaLibrary[i]))
				videoColor[mediaLibrary[i]] =
					Constants.colorDefaults[i % Constants.colorDefaults.Length]
						.Key; //Will catch new oggs that are in the mediaLibrary directory and not in the existing color directory.
	}

	private static void LoadMediaLibrary()
	{
		var files = Directory.GetFiles(libraryDir, "*.*");
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

	public static Vector2 saveWindowSize = new Vector2(Settings.ScreenW * 0.5f, Settings.ScreenH * 0.5f);
}
