using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VideoPlaying;

public static class Settings
{
    public static float ScreenW = 1024, ScreenH = 768, slideInterval = 15;
    public static int initialScreenWidth;
    public static string heartbeatServerAddress = "www.dtimotions.com/checkin.php",
        //{get{return "www.dtimotions.com/checkin.php?l="+clientLogin;}}/*lighterPath="lighter",*/
        //heartbeatServerAddress="www.sauerburger.org/dti/checkin.php",
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
    public static string[] library;
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
    public static KeyValuePair<string, Color32>[] colorDefaults = new KeyValuePair<string, Color32>[]{//To retrieve key by index: colorDefaults.Cast<DictionaryEntry>().ElementAt(index);
		new KeyValuePair<string,Color32>("maroon",new Color32(128,0,0,255)),
        new KeyValuePair<string,Color32>("firebrick",new Color32(178,34,34,255)),
        new KeyValuePair<string,Color32>("crimson",new Color32(220,20,60,255)),
        new KeyValuePair<string,Color32>("red",new Color32(255,0,0,255)),
        new KeyValuePair<string,Color32>("tomato",new Color32(255,99,71,255)),
        new KeyValuePair<string,Color32>("coral",new Color32(255,127,80,255)),
        new KeyValuePair<string,Color32>("orange",new Color32(255,165,0,255)),
        new KeyValuePair<string,Color32>("gold",new Color32(255,215,0,255)),
        new KeyValuePair<string,Color32>("yellow",new Color32(255,255,0,255)),
        new KeyValuePair<string,Color32>("yellow green",new Color32(154,205,50,255)),
        new KeyValuePair<string,Color32>("green yellow",new Color32(173,255,47,255)),
        new KeyValuePair<string,Color32>("green",new Color32(0,128,0,255)),
        new KeyValuePair<string,Color32>("lime green",new Color32(50,205,50,255)),
        new KeyValuePair<string,Color32>("light green",new Color32(144,238,144,255)),
        new KeyValuePair<string,Color32>("spring green",new Color32(0,255,127,255)),
        new KeyValuePair<string,Color32>("medium aqua marine",new Color32(102,205,170,255)),
        new KeyValuePair<string,Color32>("light sea green",new Color32(32,178,170,255)),
        new KeyValuePair<string,Color32>("cyan",new Color32(0,255,255,255)),
        new KeyValuePair<string,Color32>("turquoise",new Color32(64,224,208,255)),
        new KeyValuePair<string,Color32>("pale turquoise",new Color32(175,238,238,255)),
        new KeyValuePair<string,Color32>("corn flower blue",new Color32(100,149,237,255)),
        new KeyValuePair<string,Color32>("deep sky blue",new Color32(0,191,255,255)),
        new KeyValuePair<string,Color32>("dodger blue",new Color32(30,144,255,255)),
        new KeyValuePair<string,Color32>("light sky blue",new Color32(135,206,250,255)),
        new KeyValuePair<string,Color32>("navy",new Color32(0,0,128,255)),
        new KeyValuePair<string,Color32>("blue",new Color32(0,0,255,255)),
        new KeyValuePair<string,Color32>("royal blue",new Color32(65,105,225,255)),
        new KeyValuePair<string,Color32>("indigo",new Color32(75,0,130,255)),
        new KeyValuePair<string,Color32>("medium slate blue",new Color32(123,104,238,255)),
        new KeyValuePair<string,Color32>("medium purple",new Color32(147,112,219,255)),
        new KeyValuePair<string,Color32>("dark magenta",new Color32(139,0,139,255)),
        new KeyValuePair<string,Color32>("dark orchid",new Color32(153,50,204,255))
//033		medium orchid	#BA55D3	186,85,211	DID NOT MAKE IT IN TO CureCore List
//034		plum	#DDA0DD	221,160,221	DID NOT MAKE IT IN TO CureCore List
//035		violet	#EE82EE	238,130,238	DID NOT MAKE IT IN TO CureCore List
//036		magenta / fuchsia	#FF00FF	255,0,255	DID NOT MAKE IT IN TO CureCore List
//037		deep pink	#FF1493	255,20,147	DID NOT MAKE IT IN TO CureCore List
//038		hot pink	#FF69B4	255,105,180	DID NOT MAKE IT IN TO CureCore List
//039		pink	#FFC0CB	255,192,203	DID NOT MAKE IT IN TO CureCore List
//040		corn silk	#FFF8DC	255,248,220	DID NOT MAKE IT IN TO CureCore List
//041		misty rose	#FFE4E1	255,228,225	DID NOT MAKE IT IN TO CureCore List
//042		lavender blush	#FFF0F5	255,240,245	DID NOT MAKE IT IN TO CureCore List
//043		lavender	#E6E6FA	230,230,250	DID NOT MAKE IT IN TO CureCore List
//044		honeydew	#F0FFF0	240,255,240	DID NOT MAKE IT IN TO CureCore List
//045		azure	#F0FFFF	240,255,255	DID NOT MAKE IT IN TO CureCore List
//046		white	#FFFFFF	255,255,255	DID NOT MAKE IT IN TO CureCore List
    };
    public static bool sound = true;
    public static bool _persist = true;
    public static bool rotation = true;
    public static float volume = 1.0f;
#if UNITY_EDITOR
    public static string dataPath = "meshes";
#elif UNITY_STANDALONE_LINUX
    public static string dataPath="/home/motions/app/meshes";
#else
//public static string dataPath="C:\\motions\\meshes";
  public static string dataPath="meshes";
#endif
#if UNITY_EDITOR
    public static string categoryFile = "categories.cfg"/*,unlockFile="unlocked.cfg"*/, configFile = "motions.cfg", commandFile = "cmd", libraryDir = "meshes", patientDir = "patient", thumbsDir = "Thumbs", testBackground = "file://Test.jpg", movieColorFile = "moviecolors.cfg";
#elif UNITY_STANDALONE_LINUX
  public static string categoryFile="/etc/motions/categories.cfg",unlockFile="/etc/motions/unlocked.cfg",configFile="/etc/motions/motions.cfg",commandFile="/var/www/run/motions.cmd",libraryDir="/usr/share/motions/Movies",patientDir="/var/www/html/images/patient",thumbsDir="/var/www/html/images/thumbs",appDir="/home/motions/app",testBackground="file://"+appDir+"/Test.jpg",movieColorFile="/etc/motions/moviecolors.cfg";
#else
  public static string categoryFile="C:\\motions\\categories.cfg",unlockFile="C:\\motions\\unlocked.cfg",configFile="C:\\motions\\motions.cfg",commandFile="C:\\motions\\cmd",libraryDir="Movies",patientDir="patient",thumbsDir="Thumbs",testBackground="file:///Test.jpg",movieColorFile="C:\\motions\\moviecolors.cfg";
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
        if (!Directory.Exists(libraryDir)) Debug.LogError("Library directory \"" + libraryDir + "\" not found.");
        library = Directory.GetFiles(libraryDir, "*.*").Select(f => Path.GetFileName(f)).ToArray();

        if (File.Exists(movieColorFile)) videoColor = LoadMovieColors(movieColorFile);
        for (int i = 0; i < library.Length; i++)
            if (!videoColor.ContainsKey(library[i]))
                videoColor[library[i]] = colorDefaults[i % colorDefaults.Length].Key;//Will catch new oggs that are in the library directory and not in the existing color directory.
    }

    //public static string GetVideoFolder()
    //{
    //    string errorMessage = "ERROR";
    //    string videoFolderName = "Videos";

    //    string[] folders = Application.dataPath.Split(new char[] { '/' });
    //    string baseProjectFolder = "I-Motions";
    //    string videoPath = "";
    //    bool foundIMotions = false;

    //    foreach (var folder in folders)
    //    {
    //        videoPath += (folder + "/");

    //        if (folder.Equals(baseProjectFolder))
    //        {
    //            videoPath += videoFolderName + "/";
    //            foundIMotions = true;
    //            break;
    //        }
    //    }

    //    if (foundIMotions && !Directory.Exists(videoPath))
    //    {
    //        Debug.LogError("Video directory does not exist!");
    //        return errorMessage;
    //    }

    //    return foundIMotions ? videoPath : errorMessage;
    //}

    private static Dictionary<string, string> LoadMovieColors(string movieColorFile)
    {
	    Debug.Log("Removed code LoadMovieColors");

        Debug.Log("LoadMovieColors(\"" + movieColorFile + "\")");
        string line;
        StreamReader reader;
        Dictionary<string, string> movieColors = new Dictionary<string, string>();
        try
        {
            reader = new StreamReader(movieColorFile);
            int c = 0;
            while ((line = reader.ReadLine()) != null)
            {
                //Debug.Log("Line " + (c++) + ": \"" + line + "\", movieColors: " + movieColors.Stringify());
                string[] mc = line.Trim().Split(":"[0]);//Should be moviename:colorname
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
            Debug.LogError("Error loading file " + categoryFile + ": " + e.ToString());
        }
        return movieColors;
    }
}
