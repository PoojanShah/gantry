using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using System.Net;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;
using Object = System.Object;

public class Menu : Singleton<Menu>
{
    public static KeyValuePair<GUIContent, Action>[] mainMenu = {
            new KeyValuePair<GUIContent,Action>(new GUIContent("Play"),()=>SetMenu(categoryMenu,null,instance.categoryFooter,instance.gantrySkin.customStyles[1])),
            new KeyValuePair<GUIContent,Action>(new GUIContent("Edit Contour Map"),()=>EditContour(0)),
            new KeyValuePair<GUIContent,Action>(new GUIContent(Projection.numScreens>1?"Edit Wall Map":"Disabled"),()=>EditContour(1)),
            new KeyValuePair<GUIContent,Action>(new GUIContent("Options"),()=>{superPass="";Displayer=OptionsMenu;}),
            new KeyValuePair<GUIContent,Action>(new GUIContent("Library"),()=>{
                //RefreshLibrary();
                //instance.StartCoroutine(LoadThumbs());
                //Displayer=LibraryMenu;mainMenu
            }),
            new KeyValuePair<GUIContent,Action>(new GUIContent("Exit"),()=>{
                Application.Quit();
            }),
        }, categoryMenu;
    private static string adminPass = "", superPass = "", correctAdminPass = "Kim41", correctSuperPass = "Jas375";//"dti873"; _Motions01_+ Motions01
    public static bool limbo;
    public static int heartbeatTriesRemaining = Settings.allowConnectionAttempts;
    public static Action Displayer;//=()=>ShowMenu(mainMenu);

    private static bool standbyForCommands = true;
    private static Action displayerWas;
    private static bool wrongPass, passPrompt;
    private static Vector2 windowDragOffset = -Vector2.one;
    private static Vector2 libraryScroll = Vector2.zero;
    private static Texture2D[] thumbs;
    private static bool showExtensions;
    private static KeyValuePair<GUIContent, Action>[] menu;
    private static float backButtonMargin = 16;
    private static bool loadingMovie = false;
    private static string[] screenNames = { "gantry", "wall", "gantrywall" };
    public static bool _drawUI;

    public GameObject UIObject, QuitConfirmation, AdminLogin, AdminMenu;
    public InputField PassInputField;
    public LibraryScreen LibMenu;
    public Projection projection;
    public ContourEditorUI ContourUI;
    public GameObject menuBackground;
    public GUISkin gantrySkin;
    public Texture2D illuminationsHeader, categoryFooter, mediaFooter, backArrow, blankScreen, adminButton;
    public VideoPlayer[] testMovies;
    public AudioClip testAudioClip;
    public Texture2D exitButton;
    public static Rect windowPosition;
    public static Vector2 saveWindowSize = new Vector2(Settings.menuScreenW * 0.5f, Settings.ScreenH * 0.5f);
    public GameObject movieButtonPrefab;
    public Texture2D missingIconTexture;

    public static bool DraggingWindow
    {
        get { return windowDragOffset != -Vector2.one; }
        set
        {
            windowDragOffset = value ? (Vector2) SRSUtilities.adjustedFlipped - windowPosition.position : -Vector2.one;
            Debug.Log("windowDragOffset set to: " + windowDragOffset + " (value: " + value + ")");
        }
    }

    private static Rect BackButtonRect
    {
        get { return new Rect(backButtonMargin, Settings.ScreenH - 56 - backButtonMargin, 96, 56); }
    }

    private void Awake()
    {
#if UNITY_EDITOR
        //Nate: Added this so saving and video playback works on my local machine. 
        //Settings.libraryDir = Settings.GetVideoFolder();
        //Settings.appDir = Application.persistentDataPath;
        //Settings.noPersistFile = Settings.appDir + SRSUtilities.slashChar + "halt.motions";
#endif
        foreach (Func<IEnumerator> f in new Func<IEnumerator>[] { UpdateCheck, DongleCheck }) StartCoroutine(f());
        Settings.Load();
        Settings.LoadLibraryAndCategories();
        Settings.initialScreenWidth = Screen.currentResolution.width;//Screen.width;
                                                                     //Settings.dongleKeys=new string[]{"DE2E19C984E2925D","D85D6EA1539B7493"};
        Debug.Log("Initial ScreenWidth: " + Settings.initialScreenWidth);
        (projection = projection ?? FindObjectOfType<Projection>()).gameObject.SetActive(false);
        Texture2D[] categoryTextures = Resources.LoadAll<Texture2D>("categories");
        List<KeyValuePair<GUIContent, Action>> catList = new List<KeyValuePair<GUIContent, Action>>();
        for (int i = 0; i < categoryTextures.Length; i++)
        {
            int _i = i;
            catList.Add(new KeyValuePair<GUIContent, Action>(new GUIContent(categoryTextures[i]), () =>
            {
                LoadCategoryMenu(_i);
                Displayer = ShowPlayer;
            }));
        }
        categoryMenu = catList.ToArray();

        //Nate: I'm guessing this is here to call the awake function for Projection.
        projection.gameObject.SetActive(true);
        projection.gameObject.SetActive(false);
        StartCoroutine(CheckForCommands());

        Settings.monitorMode = Settings.MonitorMode.Single;
        Debug.Log("Fullscreen was: " + Screen.fullScreen);
        Screen.fullScreen = false;

        bool onStartRan = false;
        if (File.Exists(Settings.configFile))
        {//Format: variable name on the left, followed by "=", followed by value.
            Debug.Log("Config file \"" + Settings.configFile + "\" found.");
            StreamReader reader = new StreamReader(Settings.configFile);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("#")) continue;
                if (!line.Contains("=") || line.Split("="[0])[1].Trim().Length < 1)
                {
                    Debug.LogError("Ungueltiges line: \"" + line + "\"");
                    continue;
                }
                switch (line.Split("="[0])[0].Trim().ToLower())
                {
                    case "commandfile":
                        Settings.commandFile = line.Split("="[0])[1].Trim();
                        Debug.Log("Set command file to \"" + Settings.commandFile + "\".");
                        break;
                    case "onstart":
                        RunCommand(line.Split("="[0])[1].Trim());
                        onStartRan = true;
                        break;
                    case "persist":
                        Settings.persist = Boolean.Parse(line.Split("="[0])[1].Trim());
                        break;
                    case "simplemenu":
                        Settings._simpleMenu=Convert.ToBoolean(Int32.Parse(line.Split("="[0])[1].Trim())==1);//Settings.simpleMenu=Boolean.Parse(line.Split("="[0])[1].Trim());
                        break;
                    default:
                        Debug.LogError("Ungueltiges config file entry: \"" + line.Split("="[0])[0] + "\"");
                        break;
                }
            }
        }
        else Debug.Log("Config file \"" + Settings.configFile + "\" not found.");

        Debug.Log("onStartRan: " + onStartRan);
#if UNITY_EDITOR
        if (!onStartRan) SetMenu();
#else
        if(!onStartRan)SetMenu(categoryMenu);
#endif
    }

    private void OnGUI()
    {
        if (!_drawUI) return;
        GUI.skin = gantrySkin;
        Settings.NormalizeGUIMatrix();
        if (Displayer != null) Displayer();
    }

    private void Update()
    {
        if (projection.playing && Input.GetKeyDown(KeyCode.Escape))
        {

            ContourEditor.WipeBlackouts();
            //SetMenu(categoryMenu);
            projection.playMode = false;
            UIObject.SetActive(true);

            instance.menuBackground.SetActive(false);
            instance.DestroyPreviews();
            instance.projection.gameObject.SetActive(false);
            Camera.main.transform.Find("Scrolling Background").gameObject.SetActive(true);
            FindObjectsOfType(typeof(VideoPlayer)).ToList().ForEach((mto) => { Debug.Log("Stopping \"" + mto.name + "\"."); (mto as VideoPlayer).Stop(); });
            Settings.ShowCursor();
            //Displayer = () => EditContour(0);
            //superPass = "";
            //Displayer = OptionsMenu;
        }

        if (DraggingWindow) windowPosition.position = (Vector2) SRSUtilities.adjustedFlipped - windowDragOffset;
    }

    public void AdministratorLogin()
    {
        AdminLogin.SetActive(true);
    }

    public void ShowQuitUI()
    {
        QuitConfirmation.SetActive(true);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void AdminPassChanged(string pass)
    {
        adminPass = pass;
    }

    public void OnAdminLogin()
    {
        if (adminPass != correctAdminPass) return;
        AdminMenu.SetActive(true);
        AdminLogin.SetActive(false);
        PassInputField.text = "";
        adminPass = "";
    }

    public void ShowLibrary()
    {
        LibMenu.gameObject.SetActive(true);
        Settings.LoadLibraryAndCategories();
        LibMenu.ShowLibraryOptions();
    }

    public void EditContour()
    {
        if (ContourEditor.HideOldUI)
        {
            ContourUI.OnDisplayContourEditor();
            //EditContour(0);
            Displayer = () => EditContour(0);
            _drawUI = true;
        }
        else
        {
            Displayer = () => EditContour(0);
            _drawUI = true;
            UIObject.SetActive(false);
        }
    }

    public void ShowOptions()
    {
        superPass = "";
        Displayer = OptionsMenu;
        UIObject.SetActive(false);
        _drawUI = true;
    }

    public void QuitPressed()
    {
        Application.Quit();
    }

    public static void ResetWindowPosition()
    {
        Debug.Log("Menu.ResetWindowPosition() Settings.menuScreenW: " + Settings.menuScreenW + ", saveWindowSize.x: " + saveWindowSize.x);
        windowPosition = new Rect(Settings.menuScreenW * 0.5f - saveWindowSize.x * 0.5f, Screen.height * 0.5f - saveWindowSize.y * 0.5f, saveWindowSize.x, saveWindowSize.y);
    }

    private static string[][] ReadConfigFile(string fileName)
    {
        Debug.Log("ReadConfigFile(" + fileName + ")");
        if (!File.Exists(fileName))
        {//Format: variable name on the left, followed by "=", followed by value.
            Debug.LogWarning("Config file \"" + fileName + "\" not found.");
            return null;
        }
        string[][] ergebnis = new string[4][];
        StreamReader reader = new StreamReader(fileName);
        string line;
        int l = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Trim().StartsWith("#")) continue;
            ergebnis[l++] = line.Split(","[0]);
        }
        reader.Close();
        if (l != 4) Debug.LogWarning("Category file is corrupt with " + l + " lines.");
        return ergebnis;
    }

    private static bool WriteConfigFile(string fileName, string[][] data)
    {
        Debug.Log("WriteConfigFile(" + fileName + ")");
        try
        {
            StreamWriter sw = new StreamWriter(fileName);
            for (int i = 0; i < data.Length; i++) sw.WriteLine(string.Join(",", data[i]));
            sw.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing to file " + fileName + ": " + e.Message);
            return false;
        }
        return true;
    }

    private IEnumerator UpdateCheck()
    {//Security feature
        //Settings.version=1;//TMP
        while (true)
        {
            if (!Settings.serverCheck)
            {
                Debug.Log("---Security checks off at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; silently looping.---");
                yield return new WaitForSeconds(Settings.updatePeriod);
                continue;
            }
            Debug.Log("--------------------------------------\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Menu.UpdateCheck() connecting to: " + Settings.heartbeatServerAddress);
            WWWForm form = new WWWForm();
            form.AddField("l", Settings.clientLogin);
            form.AddField("version", Settings.version);
            WWW w = new WWW(Settings.heartbeatServerAddress, form.data);
            yield return w;
            string ergebnis = w.text.Split("\n"[0])[0];
            Debug.Log("Server returned with:\n" + w.text + "\n\nergebnis: \"" + ergebnis + "\"\ncomparison: \"Gesetzlich\", fehler: \"" + w.error + "\"\n\n");
            string/*int*/[] unlocked = null;
            int serverVersion;
            if (!ergebnis.StartsWith("Gesetzlich") || !string.IsNullOrEmpty(w.error) || !int.TryParse(w.text.Split("\n"[0])[1], out serverVersion) || (w.text.Split("\n"[0]).Length > 2 && (unlocked = w.text.Split("\n"[0])[2].Split(","[0])/*.Select(n=>Convert.ToInt32(n))*/.ToArray()) == null))
            {
                Debug.LogWarning("Fehler mit verbindung. Ergebnis: \"" + ergebnis + "\"; error: \"" + w.error + "\", tries remaining: " + heartbeatTriesRemaining + "\n\n");
                if (--heartbeatTriesRemaining <= 0)
                {
                    Debug.LogWarning("Ran out of tries; quitting and creating \"" + Settings.noPersistFile + "\".");
                    try
                    {
                        File.Create(Settings.noPersistFile).Dispose();
                    }
                    catch (IOException e)
                    {
                        Debug.LogError(e);
                    }

                    Application.Quit();
                    yield break;
                }
            }
            else
            {
                heartbeatTriesRemaining = Settings.allowConnectionAttempts;
                Debug.Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Server version: " + serverVersion + ", our version: " + Settings.version + ", neuer: " + (serverVersion > Settings.version) + ", unlocked: \"" + unlocked.Stringify() + "\"\n--------------------------------------\n");
                if (serverVersion > Settings.version) StartCoroutine(UpdateVersion(serverVersion));
                for (int i = 0; i < unlocked.Length; i++)
                {//Each is in format: 1-char BGMW bitfield, id, media extension abbreviation, thumb extension abbreviation.
                    bool[] bgmw = new bool[4];
                    for (int j = 0; j < bgmw.Length; j++) bgmw[j] = Convert.ToBoolean(Convert.ToInt32(unlocked[i].Substring(0, 1), 16));
                    string ext = Regex.Replace(unlocked[i], @"^.\d+([^\d])\w$", "$1"),
                        fn = Settings.libraryDir + SRSUtilities.slashChar + Regex.Replace(unlocked[i], @"^.(\d+)[A-Za-z]+$", "$1") + "." + (new string[] { "jpg", "png", "ogg" }.First(s => s.Substring(0, 1) == ext));
                    if (!File.Exists(fn))
                    {
                        Debug.Log("Downloading \"" + Settings.dlServeraddress + "?dl=movies/" + Path.GetFileName(fn) + "\" to \"" + fn + "\".");
                        new WebClient().DownloadFileAsync(new Uri(Settings.dlServeraddress + "?dl=movies/" + Path.GetFileName(fn)), fn);
                    }
                    else Debug.Log("\"" + fn + "\" exists.");
                    ext = Regex.Replace(unlocked[i], @"^.\d+[^\d](\w)$", "$1");
                    fn = Settings.thumbsDir + SRSUtilities.slashChar + Regex.Replace(unlocked[i], @"^.(\d+)[A-Za-z]+$", "$1") + "." + (new string[] { "jpg", "png", "ogg" }.First(s => s.Substring(0, 1) == ext));
                    if (!File.Exists(fn))
                    {
                        Debug.Log("Downloading \"" + Settings.dlServeraddress + "?dl=thumbs/" + Path.GetFileName(fn) + "\" to \"" + fn + "\".");//Debug.Log("Downloading \""+Settings.dlServeraddress+"?dl="+Path.GetFileName(fn)+"&thumb"+"\" to \""+fn+"\".");
                        new WebClient().DownloadFileAsync(new Uri(Settings.dlServeraddress + "?dl=thumbs/" + Path.GetFileName(fn)), fn);//new WebClient().DownloadFileAsync(new Uri(Settings.dlServeraddress+"?dl="+Path.GetFileName(fn)+"&thumb"),fn);
                    }
                    Debug.Log("\"" + fn + "\" exists.");
                }
                yield return new WaitForSeconds(Settings.updatePeriod);
            }
        }
    }

    private static IEnumerator UpdateVersion(int toVersion)
    {
        Debug.Log("Menu.Update(" + toVersion + ")");
        string bkpPath = Settings.binaryPath + ".bkp";
        while (File.Exists(bkpPath)) bkpPath += "I";
        //PlayerPrefs.SetInt("Version",toVersion);
        Settings.version = toVersion;
        //Process.Start(Settings.binaryPath);
        WWW w = new WWW(Settings.newBinaryURL);
        yield return w;
        Debug.Log("Downloaded. Moving: " + Settings.binaryPath + " to " + bkpPath);
        File.Move(Settings.binaryPath, bkpPath);
        File.WriteAllBytes(Settings.binaryPath, w.bytes);
        Process.Start(Settings.binaryPath);
        Application.Quit();
    }

    private IEnumerator DongleCheck()
    {//Security feature
        Process p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Settings.dongleChecker,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        while (true)
        {
            yield return new WaitForSeconds(Settings.dongleCheckInterval);//Allow us the initial period to get to the admin panel if we just started up.
            if (!Settings.dongleCheck || Displayer == OptionsMenu)
            {
                Debug.Log("==================Skipping dongle security checks at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ". dongleCheck: " + Settings.dongleCheck + "; in options menu: " + (Displayer == OptionsMenu) + "; silently looping.---");
                continue;
            }
            Debug.Log("==================\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Menu.DongleCheck() probing USB port with keys: " + string.Join(",", Settings.dongleKeys));

            if (!File.Exists(Settings.dongleChecker))
            {
                StartCoroutine(ReportAndQuit(Settings.heartbeatServerAddress, "No dongle checker binary \"" + Settings.dongleChecker + "\"."));
                yield break;
            }

            p.StartInfo.Arguments = string.Join(" ", Settings.dongleKeys);
            p.Start();

            while (!p.StandardOutput.EndOfStream)
            {
                string line = p.StandardOutput.ReadLine();//alternative: string output=p.StandardOutput.ReadToEnd();p.WaitForExit();
                Debug.Log("Process returned: \"" + line + "\"");
                if (line != "Gesetzlich")
                {
                    StartCoroutine(ReportAndQuit(Settings.heartbeatServerAddress, "Fehler mit dongle authentication; quitting and creating " + Settings.noPersistFile + "."));
                    yield break;
                }
                else Debug.Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Dongle check success.");
            }
        }
    }

    public static IEnumerator ReportAndQuit(string server, string msg)
    {
        Debug.Log("Menu.ReportAndQuit(" + server + "," + msg + ")");
        WWWForm form = new WWWForm();
        form.AddField("msg", msg);
        form.AddField("l", Settings.clientLogin);
        WWW w = new WWW(server, form.data);
        yield return w;
        string ergebnis = w.text.Split("\n"[0])[0];
        Debug.Log("Server \"" + server + "\" returned with:\n" + w.text + "\n\nergebnis: \"" + ergebnis + "\"\nerror: \"" + w.error + "\"\n");
        File.Create(Settings.noPersistFile);
        Application.Quit();
    }
    
    private IEnumerator CheckForCommands()
    {
        Debug.Log("Menu.CheckForCommands(), standing by: " + standbyForCommands);
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (standbyForCommands && File.Exists(Settings.commandFile))
            {
                Debug.Log("Command file found.");
                //				RunCommand(File.ReadAllText(commandFile));
                foreach (string command in File.ReadAllText(Settings.commandFile).Trim().Split("\n"[0])) RunCommand(command.Trim());
                File.Delete(Settings.commandFile);
            }
        }
    }
    
    private static void RunCommand(string command)
    {
        Debug.Log("Running command: \"" + command + "\".");
        int screenNum;
        switch (command.Split(":"[0])[0].Trim())
        {
            case "wiedergaben":
                
                string movieName = command.Split(":"[0]).Last<string>().Trim();
                screenNum = command.Split(":"[0]).Length > 2 ? Array.IndexOf(screenNames, command.Split(":"[0])[1].Trim()) : 0;
                
                string pattern = "^Test.jpg(\\.(jpg|png|ogg))?$";
                string subject = "Test";
                
                Debug.Log("Test: \"" + subject + "\", \"" + Regex.Replace("Test.jpg", "\\.\\w{3,4}$", "") + "\" / " + pattern + "   : " + Regex.IsMatch(Regex.Replace(subject, "\\.\\w{3,4}$", ""), pattern, RegexOptions.IgnoreCase));
                
                Debug.Log("Categories: " + string.Join("\n", Settings.categories.Select(l => string.Join(",", l)).ToArray()));
                
                if (Settings.categories.Any(a => a.Any(n => Regex.IsMatch(n, "^" + Regex.Replace(movieName, "\\.\\w{3,4}$", "") + "(\\.(jpg|png|ogg))?$", RegexOptions.IgnoreCase))))
                {
                    //StartMovie(movieName,(int)Settings.screenMode);
                    instance.projection.StartMovie(movieName, screenNum);
                }
                else
                {
                    Debug.LogWarning("Attempted to play locked or unrecognized movie \"" + movieName + "\".");
                    instance.projection.StartMovie(movieName, screenNum);
                }
                break;
            case "playpatient"://Show patient photos
                string patientName = command.Split(":"[0]).Last<string>().Trim();
                screenNum = command.Split(":"[0]).Length > 2 ? Array.IndexOf(screenNames, command.Split(":"[0])[1].Trim()) : 0;
                Debug.Log("Playing slides of patient \"" + patientName + "\" on screen " + screenNum + ".");
                instance.projection.StartSlideshow(patientName, screenNum);
                break;
            case "halt":
                if (instance.projection.playing) instance.projection.StopMovie(command.Split(":"[0]).Length > 1 ? Array.IndexOf(new string[] { "gantry", "wall" }, command.Split(":"[0])[1].Trim()) : -1);//"gantrywall" will return -1 from Array.IndexOf.
                limbo = true;
                Projection.currentSlideLoop++;
                break;
            case "screen":
                Settings.screenMode = (Settings.ScreenMode)Enum.Parse(typeof(Settings.ScreenMode), command.Split(":"[0])[1].Trim());
                break;
            case "rotate":
                Projection.Rotate(command.Split(":"[0])[1] == "Wall" ? 1 : 0);
                break;
            default:
                Debug.LogError("Ungueltiges kommand: " + command.Split(":"[0])[0].Trim() + " (from command: " + command + ")");
                break;
        }
    }

    private void DestroyPreviews()
    {
        Debug.Log("Menu.DestroyPreviews()");
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }
    
    private void LoadCategoryMenu(int category = 0)
    {
        float margin = 0.5f;
        for (int i = 0; i < Settings.categories[category].Length; i++)
        {
            int columns = 2;
            float width = 2.5f;
            GameObject buttonObj = GameObject.Instantiate(movieButtonPrefab, Camera.main.transform.position + Vector3.down * 5 + Vector3.left * width * 0.5f + Vector3.right * width / (columns - 1) * (i % columns) + Vector3.forward * 2 + Vector3.back * (columns + margin) * ((int)(i / columns)), Quaternion.Euler(90, 0, 0)) as GameObject;
            buttonObj.transform.parent = transform;
            buttonObj.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>(Settings.thumbsDir + "/" + Settings.categories[category][i]);
            buttonObj.transform.Find("Label").GetComponent<TextMesh>().text = buttonObj.GetComponent<PreviewButton>().movieName = Settings.categories[category][i];
        }
    }

    private static void Overlays(Texture2D footerTexture)
    {
        GUI.DrawTexture(new Rect(0, 0, Settings.ScreenW, Settings.ScreenH * 0.127617148554337f), instance.illuminationsHeader);
        GUI.DrawTexture(new Rect(0, Settings.ScreenH * (1 - 0.1246261216350947f), Settings.ScreenW, Settings.ScreenH * 0.1246261216350947f), footerTexture);
    }
    
    private static void DrawConfirmQuit()
    {
        GUI.Window(0, windowPosition, (id) =>
        {
            GUI.Label(new Rect(windowPosition.width * 0.25f, windowPosition.height * 0.25f, windowPosition.width * 0.5f, 32), "Really quit?", instance.gantrySkin.customStyles[2]);
            if (GUI.Button(new Rect(windowPosition.width * 0.2f, windowPosition.height * 0.75f, windowPosition.width * 0.2f, 32), "Yes")) Application.Quit();
            else if (GUI.Button(new Rect(windowPosition.width * 0.6f, windowPosition.height * 0.75f, windowPosition.width * 0.2f, 32), "No")) Displayer = displayerWas;
        }, "Confirmation");
    }
    
    private static void DrawAdminPassPrompt()
    {
        GUI.Window(0, windowPosition, (id) =>
        {
            GUI.Label(new Rect(windowPosition.width * 0.05f, windowPosition.height * 0.25f, windowPosition.width * 0.9f, 32), "Please enter the administrator password:", instance.gantrySkin.customStyles[2]);
            GUI.skin.textField.overflow.bottom = 0;
            if (Event.current != null && Event.current.type == EventType.KeyDown && new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter }.Any(kc => Event.current.keyCode == kc)) TryAdminPassword();
            GUI.SetNextControlName("Passfield");
            adminPass = GUI.PasswordField(new Rect(windowPosition.width * 0.25f, windowPosition.height * 0.25f + 64, windowPosition.width * 0.5f, 24), adminPass, "*"[0]);
            GUI.FocusControl("Passfield");
            if (adminPass.IndexOfAny("\n\r".ToCharArray()) != -1) Debug.LogWarning("Return in pass string.");
            if (wrongPass) GUI.Label(new Rect(windowPosition.width * 0.05f, windowPosition.height * 0.25f + 96, windowPosition.width * 0.9f, 32), "Incorrect password. Please try again.", instance.gantrySkin.customStyles[3]);
            if (GUI.Button(new Rect(windowPosition.width * 0.2f, windowPosition.height * 0.75f, windowPosition.width * 0.2f, 32), "Ok")) TryAdminPassword();
            else if (GUI.Button(new Rect(windowPosition.width * 0.6f, windowPosition.height * 0.75f, windowPosition.width * 0.2f, 32), "Cancel")) Displayer = displayerWas;
        }, "Confirmation");
    }

    private static void TryAdminPassword()
    {
        Debug.Log("Menu.TryAdminPassword() Trying admin password against correct pass: " + (adminPass == correctAdminPass));
        if (adminPass == correctAdminPass) SetMenu();
        else wrongPass = true;
        adminPass = "";
    }

    private static string soundTemp = "";
    private static bool useSoundTemp = false;

    private static void OptionsMenu()
    {
        float lmargin = Settings.menuScreenW * 0.15f, tmargin = Settings.ScreenH * 0.15f, rowLevel = tmargin, buttonHeight = 48, rowHeight = 54,/*buttonWidth=32,*/panelWidth = 256;
        int rightOverflowWas = GUI.skin.toggle.overflow.right;
        GUI.skin.toggle.border.left = 0;
        GUI.skin.textField.overflow.bottom = -16;
        GUI.enabled = superPass != correctSuperPass;
        GUI.Label(new Rect(lmargin, rowLevel, panelWidth * 0.75f, buttonHeight * 0.5f), "Super Administrator Password: ");
        GUI.SetNextControlName("SuperAdminPass");
        superPass = GUI.PasswordField(new Rect(lmargin + panelWidth * 0.75f, rowLevel, panelWidth, buttonHeight), superPass, "*"[0], 16);

        if (GUI.enabled = superPass == correctSuperPass)
        {
            GUI.Label(new Rect(lmargin, rowLevel += rowHeight, panelWidth * 0.5f, buttonHeight * 0.5f), "Client E-mail: ");
            Settings.clientLogin = GUI.TextField(new Rect(lmargin + panelWidth * 0.5f, rowLevel, panelWidth, buttonHeight), Settings.clientLogin, 16);
            Settings.sound = Graphics.Toggle(new Rect(lmargin, rowLevel += rowHeight, panelWidth, buttonHeight),
                Settings.sound, "Sound");

            try
            {
                if (!useSoundTemp)
                {
                    Settings.volume = float.Parse(GUI.TextField(new Rect(lmargin + panelWidth * 0.5f, rowLevel, panelWidth, buttonHeight), Settings.volume + "", 4));
                }
                else
                {
                    soundTemp = GUI.TextField(new Rect(lmargin + panelWidth * 0.5f, rowLevel, panelWidth, buttonHeight), soundTemp, 4);
                    Settings.volume = float.Parse(soundTemp);
                }
            }
            catch (Exception e)
            {
                useSoundTemp = true;
            }

            
            Settings.persist = Graphics.Toggle(new Rect(lmargin, rowLevel += rowHeight, panelWidth, buttonHeight), Settings.persist, "Automatic Restart");//"Automatic Restart: O"+(Settings.persist?"n":"ff"
            Settings.rotation = Graphics.Toggle(new Rect(lmargin + panelWidth, rowLevel, panelWidth, buttonHeight), Settings.rotation, "Rotation");
            Settings.monitorMode = (Settings.MonitorMode)GUI.SelectionGrid(new Rect(lmargin, rowLevel += rowHeight, panelWidth, buttonHeight), (int)Settings.monitorMode, Enum.GetNames(typeof(Settings.MonitorMode)).Select(n => n + " Monitor").ToArray(), 2);
            Screen.fullScreen = Convert.ToBoolean(GUI.SelectionGrid(new Rect(lmargin, rowLevel += rowHeight, panelWidth, buttonHeight), Convert.ToInt32(Screen.fullScreen), new string[] { "Windowed", "Full Screen" }, 2));
            Settings.serverCheck = Graphics.Toggle(new Rect(lmargin, rowLevel += rowHeight, panelWidth, buttonHeight), Settings.serverCheck, "Online Server Checks");
            GUI.enabled = Settings.dongleCheck = Graphics.Toggle(new Rect(lmargin, rowLevel += rowHeight, panelWidth, buttonHeight), Settings.dongleCheck, "Require Dongle");

            for (int i = 0; i < Settings.dongleKeys.Length; i++)
            {
                GUI.Label(new Rect(lmargin, rowLevel += rowHeight * (1 - (float)i * 0.5f), panelWidth * 0.5f, buttonHeight * 0.5f), "SmartDongle P" + (i + 1) + ": ");
                Settings.dongleKeys[i] = GUI.TextField(new Rect(lmargin + panelWidth * 0.5f, rowLevel, panelWidth, buttonHeight), Settings.dongleKeys[i], 16);
            }

            GUI.enabled = true;
            GUI.enabled = Settings.useCueCore = Graphics.Toggle(new Rect(lmargin, rowLevel += rowHeight, 32, buttonHeight), Settings.useCueCore, "CueCore Dynamic Lighting", Vector2.one * 32);//Graphics.Toggle overwrites rect's width.
            Settings.cuecoreIP = GUI.TextField(new Rect(lmargin + panelWidth * 0.3f, rowLevel += rowHeight, panelWidth * 0.7f, buttonHeight), Settings.cuecoreIP, 15);
            Settings.cuecorePort = Convert.ToInt32(GUI.TextField(new Rect(lmargin + panelWidth * 1.15f + 8, rowLevel, panelWidth * 0.3f, buttonHeight), Settings.cuecorePort.ToString(), 5));
            for (int i = 0; i < Settings.dongleKeys.Length; i++) GUI.Label(new Rect(lmargin + i * (panelWidth + 8), rowLevel, panelWidth * 0.15f * (1 + 1.5f * (1 - i)), buttonHeight * 0.5f), new string[] { "CueCore IP", "Port" }[i] + ": ");
        }
        else GUI.FocusControl("SuperAdminPass");

        GUI.enabled = true;

        if (GUI.Button(new Rect(Settings.menuScreenW - lmargin - panelWidth, Settings.ScreenH - rowHeight * 2, panelWidth * 0.5f, buttonHeight), "Back"))
        {
            //Why save here?
            Settings.Save();
            SetMenu();
            Resources.FindObjectsOfTypeAll<Canvas>()[0].gameObject.SetActive(true);
            _drawUI = false;
        }

        GUI.skin.toggle.overflow.right = rightOverflowWas;
    }

    public static void SetMenu(KeyValuePair<GUIContent, Action>[] m = null, int[] disabled = null, Texture2D background = null, GUIStyle style = null)
    {
        Debug.Log("Menu.SetMenu(" + m + "," + disabled + "," + background + ")");
#if UNITY_IPHONE
		m=m??categoryMenu;
		background=background??instance.categoryBackground;
		style=style??instance.gantrySkin.customStyles[1];
#endif
        Camera.main.transform.position = Vector3.up * 5;//in case it's skewed from editing the contour map.
        if (instance.projection.gameObject.activeSelf) Projection.StopAllMovies();
        m = m ?? mainMenu;
        if (m == categoryMenu)
        {
            style = style ?? instance.gantrySkin.customStyles[1];
            adminPass = "";
        }

        instance.menuBackground.SetActive(m != mainMenu);
        instance.DestroyPreviews();
        instance.projection.gameObject.SetActive(false);
        Camera.main.transform.Find("Scrolling Background").gameObject.SetActive(true);
        FindObjectsOfType(typeof(VideoPlayer)).ToList().ForEach((mto) => { Debug.Log("Stopping \"" + mto.name + "\"."); (mto as VideoPlayer).Stop(); });
        Settings.ShowCursor();

        instance.transform.DestroyChildren();
        Displayer = () => ShowMenu(m ?? mainMenu, disabled, style);
        Settings.ShowCursor();
        passPrompt = false;
    }

    private static void ShowMenu(KeyValuePair<GUIContent, Action>[] m, int[] disabled = null, GUIStyle style = null)
    {
        float buttonWidth = Settings.menuScreenW * 0.5f, buttonHeight = 48, margin = 16, catButtonSize = 128;
        
        if (instance.menuBackground.activeSelf) Overlays(m == categoryMenu ? instance.categoryFooter : instance.mediaFooter);
        if (m == categoryMenu)
        {
            if (GUI.Button(BackButtonRect, instance.adminButton, style ?? GUI.skin.button))
            {
                passPrompt = true;
                displayerWas = Displayer;
                wrongPass = false;
                Displayer = DrawAdminPassPrompt;
                ResetWindowPosition();
                SRSUtilities.guiMatrixNormalized = true;
            }
            if (GUI.Button(new Rect(Settings.ScreenW - backButtonMargin - 96, Settings.ScreenH - 56 - backButtonMargin, 96, 56), instance.exitButton))
            {
                displayerWas = Displayer;
                Displayer = DrawConfirmQuit;
                ResetWindowPosition();
                SRSUtilities.guiMatrixNormalized = true;
            }
        }
        else for (int i = 0; i < m.Length; i++)
        {
            GUI.enabled = !disabled.Contains(i);
            if (m[i].Key.text != "Disabled" && GUI.Button(m == categoryMenu
                        ? new Rect(
                            Settings.menuScreenW * 0.4f + (i % 2) * Settings.ScreenW * 0.2f - catButtonSize * 0.5f,
                            Settings.ScreenH * 0.35f + i / 2 * Settings.ScreenH * 0.3f - catButtonSize * 0.5f,
                            catButtonSize, catButtonSize)
                        : new Rect((Settings.menuScreenW - buttonWidth) * 0.5f,
                            (Settings.ScreenH - m.Length * (buttonHeight + margin)) * 0.5f +
                            i * (buttonHeight + margin), buttonWidth, buttonHeight), m[i].Key,
                    style ?? GUI.skin.button))
            {
                m[i].Value();
            }
        }

        GUI.enabled = true;
    }
    
    private static void EditContour(int screenNum)
    {
        Debug.Log("Menu.EditContour(" + screenNum + ")");
        Displayer = null;
        instance.projection.transform.gameObject.SetActive(true);
        Projection.editing = true;//Keep before ContourEditor initialization.
        instance.menuBackground.SetActive(false);
        Camera.main.transform.Find("Scrolling Background").gameObject.SetActive(false);
        instance.projection.enabled = true;
        Camera.main.transform.position = -Projection.ScreenPosition(screenNum) + Vector3.up * 5;
        Settings.monitorMode = (Settings.MonitorMode)screenNum;
        instance.projection.GetComponent<Toolbar>().enabled = instance.projection.GetComponent<InfoDisplay>().enabled = true;
        ContourEditor.Reset();//after toolbar's Awake, so it can select.
        ContourEditor.Restart();
    }

    public static void ShowPlayer()
    {
        if (/*loaded&&*/instance.transform.childCount < 1 && !instance.projection.gameObject.activeSelf)
            GUI.Label(new Rect(Settings.ScreenW * 0.5f - 64, Settings.ScreenH * 0.5f - 32, 128, 64), loadingMovie ? "Loading..." : "There are no movies available at this time.");
        else if (instance.projection.gameObject.activeSelf) ContourEditor.DrawBlackouts(true);
        
        if (!instance.projection.playing && !limbo)
        {
            Overlays(instance.mediaFooter);
            if (GUI.Button(BackButtonRect, instance.backArrow, instance.gantrySkin.customStyles[1]))
            {
                Debug.Log("Should be destroying previews...");
                instance.DestroyPreviews();
                SetMenu(categoryMenu, null, instance.categoryFooter, instance.gantrySkin.customStyles[1]);
                Settings.ShowCursor(true);
            }
        }
    }
}
