using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core;
using VideoPlaying;

public static class Settings
{
    public static float ScreenW = 1024, ScreenH = 768, slideInterval = 15;
    public static int initialScreenWidth;
    public static string heartbeatServerAddress = "www.dtimotions.com/checkin.php",
	    clientLogin = "", dlServeraddress = "http://www.dtimotions.com",
#if UNITY_EDITOR
        appDir = ".",
        binaryFile = "Motions-Linux.x86_64",
#elif UNITY_STANDALONE_LINUX
        appDir="/home/motions/app",
        binaryFile="Motions-Linux.x86_64",
#else
        appDir=".",
        binaryFile="Motions-Win64.exe",
        //binaryPath=binaryFile,
#endif
        binaryPath = appDir + SRSUtilities.slashChar + binaryFile,
        noPersistFile = appDir + SRSUtilities.slashChar + "halt.motions",
        newBinaryURL = "http://www.sauerburger.org/dti/" + binaryFile;
    public static string[] mediaLibrary;
    public static float menuScreenW { get { return ScreenW / Mathf.Max(1, Projection.DisplaysAmount); } }
    public static float updatePeriod = 60, dongleCheckInterval = 30;//*60*1;//Check the server every minute or hour.
    public static int allowConnectionAttempts = 2;//24;
    public static Dictionary<string, string> videoColor = new Dictionary<string, string>();
    public static string cuecoreIP = "192.168.1.10", dongleChecker = appDir + SRSUtilities.slashChar + "motionsdongle";
    public static int cuecorePort = 7000;
    public static bool serverCheck = true, dongleCheck = true, useCueCore = false,_simpleMenu=false;//Whether or not to shut down if we don't hear from the master server. We only turn off for demonstration environments.
    public static bool simpleMenu{get{//return Convert.ToBoolean(PlayerPrefs.GetInt("SimpleMenu"));
            return _simpleMenu;
        }
        set{
            //PlayerPrefs.SetInt("SimpleMenu",value?1:0);
            if(value==_simpleMenu)return;
            string t=File.ReadAllText(configFile),nowy="simplemenu="+((_simpleMenu=value)?1:0);
            Debug.Log("Setting simpleMenu from "+_simpleMenu+" to "+value+". Text:\n"+t+"\n\nnowy: "+nowy);
            if(t.Contains("simplemenu"))t=Regex.Replace(t,"simplemenu=.*$",nowy);
            else t+="\n"+nowy;
            Debug.Log("T potem: "+t);
            File.WriteAllText(configFile,t);
        }}
    public static string[] dongleKeys = new string[] { "DE2E19C984E2925D", "D85D6EA1539B7493" };
    public static bool sound = true;
    public static bool _persist = true;
    public static bool rotation = true;
    public static float volume = 1.0f;
    public static string dataPath = Application.dataPath + "/Videos/";

#if UNITY_EDITOR
    public static string configFile = "motions.cfg", commandFile = "cmd", libraryDir = Application.dataPath + "/Videos/", patientDir = "patient", thumbsDir = "Thumbs", testBackground = "file://Test.jpg", movieColorFile = "moviecolors.cfg";
#elif UNITY_STANDALONE_LINUX
  public static string unlockFile="/etc/motions/unlocked.cfg",configFile="/etc/motions/motions.cfg",commandFile="/var/www/run/motions.cmd",libraryDir="/usr/share/motions/Movies",patientDir="/var/www/html/images/patient",thumbsDir="/var/www/html/images/thumbs",appDir="/home/motions/app",testBackground="file://"+appDir+"/Test.jpg",movieColorFile="/etc/motions/moviecolors.cfg";
#else
  public static string unlockFile="C:\\motions\\unlocked.cfg",configFile="C:\\motions\\motions.cfg",commandFile="C:\\motions\\cmd",libraryDir="Movies",patientDir="patient",thumbsDir="Thumbs",testBackground="file:///Test.jpg",movieColorFile="C:\\motions\\moviecolors.cfg";
#endif
    public enum MonitorMode { Single = 0, Dual };
    public enum ScreenMode { gantry = 0, wall, gantrywall };
    private static MonitorMode _monitorMode = MonitorMode.Single;
    public static ScreenMode _screenMode = ScreenMode.gantrywall;
    public static float originalScaleX = (float) Screen.width / (float) Screen.height;

    public static int version
    {
        get { return PlayerPrefs.HasKey("Version") ? PlayerPrefs.GetInt("Version") : 1; }
        set { PlayerPrefs.SetInt("Version", value); }
    }

    public static bool persist
    {
        get
        {
            return !File.Exists(noPersistFile);
        }
        set
        {
            if (!value && !File.Exists(noPersistFile)) File.Create(noPersistFile);
            else if (value && File.Exists(noPersistFile)) File.Delete(noPersistFile);
        }
    }

    public static MonitorMode monitorMode
    {
        get { return _monitorMode; }
        set
        {
            _monitorMode = value;
        }
    }

    public static ScreenMode screenMode
    {
        get { return _screenMode; }
        set
        {
            Debug.Log("Settings.screenMode set from " + _screenMode + " to " + value + ".");
            _screenMode = value;
        }
    }

    public static void NormalizeGUIMatrix()
    {
        SRSUtilities.NormalizeGUIMatrix();
    }
    
    public static void ShowCursor(bool show = true)
    {
        Debug.Log("Settings.ShowCursor(" + show + "); Cursor.lockState was: " + Cursor.lockState);
        if (show) Cursor.lockState = CursorLockMode.None;
        Cursor.visible = show;
        if (!show) Cursor.lockState = CursorLockMode.Locked;
    }

    public static void Save()
    {
        Debug.Log("Settings.Save()");
        PlayerPrefs.SetString("CueCoreIP", cuecoreIP);
        PlayerPrefs.SetInt("CueCorePort", cuecorePort);
        PlayerPrefs.SetInt("ServerCheck", Convert.ToInt32(serverCheck));
        PlayerPrefs.SetInt("UseCueCore", Convert.ToInt32(useCueCore));
        PlayerPrefs.SetInt("DongleCheck", Convert.ToInt32(dongleCheck));
        PlayerPrefs.SetString("DongleKeys", string.Join("", dongleKeys));
        PlayerPrefs.SetInt("Rotation", Convert.ToInt32(rotation));
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public static void Load()
    {
        Debug.Log("Settings.Load()");
        cuecoreIP = PlayerPrefs.GetString("CueCoreIP", cuecoreIP);
        cuecorePort = PlayerPrefs.GetInt("CueCorePort", cuecorePort);
        rotation = Convert.ToBoolean(PlayerPrefs.GetInt("Rotation", Convert.ToInt32(rotation)));
        serverCheck = Convert.ToBoolean(PlayerPrefs.GetInt("ServerCheck", Convert.ToInt32(serverCheck)));
        useCueCore = Convert.ToBoolean(PlayerPrefs.GetInt("UseCueCore", Convert.ToInt32(useCueCore)));
        dongleCheck = Convert.ToBoolean(PlayerPrefs.GetInt("DongleCheck", Convert.ToInt32(dongleCheck)));
        dongleKeys = new string[] { PlayerPrefs.GetString("DongleKeys", string.Join("", dongleKeys)).Substring(0, 16), PlayerPrefs.GetString("DongleKeys", string.Join("", dongleKeys)).Substring(16, 16) };
        volume = PlayerPrefs.GetFloat("Volume", volume);
    }

    public static void LoadLibraryAndCategories()
    {
        LoadMediaLibrary();

        if (File.Exists(movieColorFile)) 
	        videoColor = LoadMovieColors(movieColorFile);

        for (var i = 0; i < mediaLibrary.Length; i++)
            if (!videoColor.ContainsKey(mediaLibrary[i]))
                videoColor[mediaLibrary[i]] = Constants.colorDefaults[i % Constants.colorDefaults.Length].Key;//Will catch new oggs that are in the mediaLibrary directory and not in the existing color directory.
    }

    private static void LoadMediaLibrary()
    {
	    if (!Directory.Exists(libraryDir)) 
		    Debug.LogError("Library directory \"" + libraryDir + "\" not found.");

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
		    var reader = new StreamReader(movieColorFile);
		    int c = 0;
		    while (reader.ReadLine() is { } line)
		    {
			    string[] mc = line.Trim().Split(":"[0]); //Should be moviename:colorname
			    if (mc.Length != 2)
			    {
				    Debug.LogWarning("Ungueltiges MovieColor line: " + line);
				    continue;
			    }

			    movieColors[mc[0]] = mc[1];
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
