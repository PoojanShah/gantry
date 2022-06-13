using UnityEngine;//NOTE: the up-side-down phenomenon occurs when you haven't loaded a gantry configuration.
using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;

public class Projection : Singleton<Projection>
{
    public GameObject[] screens;
    public static bool editing { get {/*Debug.Log("instance.editor: "+instance.editor+", enabled: "+instance.editor.enabled);*/return instance.editor != null && instance.editor.enabled; } set { instance.editor.enabled = value; } }
    public static Vector3 originalExtents, rawSize = new Vector3(5, 0, 5);
    public ContourEditor editor;
    public static int currentSlideLoop = 0;

    public static int numScreens
    {
        get
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++) if (args[i] == "-num_monitors" && i < args.Length - 1) return Convert.ToInt32(args[i + 1]);
#if UNITY_EDITOR
            return 1;
#else
//return Screen.width/1024;
            return Screen.width>1024?2:1;
#endif
        }
    }

    public bool playMode
    {
        get { return playing; }
        set
        {
            for (var i = 0; i < numScreens; i++)
            {
                screens[i].SetActive(value);
            }

            GetComponent<Renderer>().enabled = !value;
        }
    }

    public bool playing
    {
        get
        {
            return gameObject.activeSelf && !editing;
        }
    }

    public string playingMovieName
    {
        get
        {
            foreach (var screen in screens)
            {
                if (screen.activeSelf && (screen.GetComponent<Renderer>().material.mainTexture as MovieTexture) != null)
                {
                    return (screen.GetComponent<Renderer>().material.mainTexture as MovieTexture).name;
                }
            }

            return "";
        }
    }

    public static Vector3 ScreenPosition(int screenNum)
    {
        Debug.Log("Projection.ScreenPosition(" + screenNum + "), numScreens: " + numScreens + ", original extents: " + Projection.originalExtents.x +
            ", local scale: " + instance.transform.localScale + ", ergebnis: " + new Vector3(originalExtents.x * instance.transform.localScale.x * (-0.5f * ((float) (numScreens - 1)) + screenNum), 0, 0));
        return new Vector3(originalExtents.x * instance.transform.localScale.x * (-1 * ((float) (numScreens - 1)) + screenNum * 2), 0, 0);
    }

    public static void StopAllMovies()
    {
        for (int i = 0; i < instance.screens.Length; i++)
        {
            MovieTexture mt = instance.screens[i].GetComponent<Renderer>().material.mainTexture as MovieTexture;
            if (mt != null) mt.Stop();
        }
    }

    public bool Playing(int screenNum)
    {
        return screenNum < 0 || screenNum > screens.Length - 1 ? screens.All<GameObject>(o => { return Playing(o); }) : Playing(screens[screenNum]);
    }

    public bool Playing(GameObject screenObj)
    {
        return screenObj != null && screenObj.activeSelf && screenObj.GetComponent<Renderer>().material.mainTexture != null;
    }

    public string PlayingMovieName(int screenNum)
    {
        return Playing(screenNum) ? (screens[0].GetComponent<Renderer>().material.mainTexture as MovieTexture).name : "";
    }

    private void Awake()
    {
        Debug.Log("Projection.Awake(); Settings.dataPath: " + Settings.dataPath + ", Application.persistentDataPath: " + Application.persistentDataPath
            + ";\nCommand Line: \"" + Environment.CommandLine + "\", Command Line Args: \"" + string.Join(",", Environment.GetCommandLineArgs()) + "\"; Screen.width: " + Screen.width + ", Screen.height: " + Screen.height + ", numScreens: " + numScreens);

        GetComponent<MeshFilter>().mesh.Clear();
        instance.transform.localScale = new Vector3(Settings.originalScaleX, 1, 1);
        originalExtents = Vector3.one * 5;
        Debug.Log("originalExtents: " + Projection.originalExtents);
        transform.localScale = new Vector3(4f / 3f, 1, 1);

        for (var i = 0; i < screens.Length; i++)
        {
            screens[i].transform.localScale = new Vector3(4f / 3f, 1, 1);
            screens[i].transform.position = ScreenPosition(i);
        }

        Debug.Log("Original Scale X: " + Settings.originalScaleX + ", Screen.width: " + Screen.width + ", screen 0 width: " + screens[0].transform.localScale.x + " (Screen.width/1024)=" + (Screen.width / 1024));
        Debug.Log("originalExtents: " + originalExtents);
        if (numScreens > 1)
        {
            Debug.Log("Screen.width: " + Screen.width + ", screen 0 width: " + screens[0].transform.localScale.x);
        }
        Debug.Log("Original Scale X 2: " + Settings.originalScaleX + ", Screen.width: " + Screen.width + ", screen 0 width: " + screens[0].transform.localScale.x + " (Screen.width/1024)=" + (Screen.width / 1024));
    }

    public void DebuggingControls()
    {
        int ypos = 0;
        
        GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "Camera pos: " + Camera.main.transform.position.x);
        Camera.main.transform.position = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
            Camera.main.transform.position.x, 2, -10, 10), Camera.main.transform.position.y, Camera.main.transform.position.z);
        GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "Camera scl: " + Camera.main.transform.localScale.x);
        Camera.main.transform.localScale = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
            Camera.main.transform.localScale.x, 2, -10, 10), Camera.main.transform.localScale.y, Camera.main.transform.localScale.z);
        GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "This pos: " + transform.position.x);
        transform.position = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
            transform.position.x, 2, -10, 10), transform.position.y, transform.position.z);
        GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "This scl: " + transform.localScale.x);
        transform.localScale = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
            transform.localScale.x, 2, -10, 10), transform.localScale.y, transform.localScale.z);

        for (int i = 0; i < screens.Length; i++)
        {
            GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "S" + i + " pos: " + screens[i].transform.position.x);
            screens[i].transform.position = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
                screens[i].transform.position.x, 2, -10, 10), screens[i].transform.position.y, screens[i].transform.position.z);
            GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "S" + i + " scale: " + screens[i].transform.localScale.x);
            screens[i].transform.localScale = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
                screens[i].transform.localScale.x, 2, -10, 10), screens[i].transform.localScale.y, screens[i].transform.localScale.z);
        }
    }
    
    public void StartMovie(string movieName, int screenNum = 0, bool testMovie = false)
    {
        Debug.Log("Projection.StartMovie(\"" + movieName + "\"," + screenNum + "," + testMovie + "); timeScale: " + Time.timeScale);
        editing = false;
        if (Playing(screenNum)) StopMovie(screenNum);
        Camera.main.transform.position = Vector3.zero + Vector3.up * 5;
        gameObject.SetActive(true);
        
        instance.StopCoroutine("LoadAndPlayExternalResource");
        instance.StartCoroutine(LoadAndPlayExternalResource(movieName, screenNum));
        instance.GetComponent<Toolbar>().enabled = instance.GetComponent<InfoDisplay>().enabled = false;
        Debug.Log("screens.Length: " + screens.Length + ", screen 2 not null: " + (screens[1] != null) + ", screens[0].transform.width: " + screens[0].transform.localScale.x);
    }

    public void StartSlideshow(string patientName, int screenNum = 0)
    {
        Debug.Log("Projection.StartMovie(\"" + patientName + "\"," + screenNum + "); timeScale: " + Time.timeScale);
        editing = false;
        if (Playing(screenNum)) StopMovie(screenNum);
        Camera.main.transform.position = Vector3.zero + Vector3.up * 5;
        gameObject.SetActive(true);
        
        instance.StopCoroutine("LoadAndPlayExternalResource");
        instance.StartCoroutine(LoadAndPlayExternalResource(patientName, screenNum, 0));
        instance.GetComponent<Toolbar>().enabled = instance.GetComponent<InfoDisplay>().enabled = false;
        Debug.Log("screens.Length: " + screens.Length + ", screen 2 not null: " + (screens[1] != null) + ", screens[0].transform.width: " + screens[0].transform.localScale.x);
    }

    private IEnumerator LoadAndPlayExternalResource(string resourceName, int screenNum = 0, int slide = -1)
    {
        //Slide of -1 is a movie; resourceName is the ogg file name if movie, patient folder name if slide.
        //stopSlides=true;//slide<0;//Clever reliance on the fact that an existing loop will finish in less time than this one, unless they happened to click this on the exact frame of it in which case it'll be equal.
        int thisLoop = ++currentSlideLoop;
        string[] slides = null, extensions = { "ogg", "jpg", "png", "" };
        Debug.Log("Projection.LoadAndPlayExternalResource(\"" + resourceName + "\"," + screenNum + "," + slide + ");");
        Menu.instance.menuBackground.SetActive(false);
        gameObject.SetActive(true);
        enabled = false;
        GetComponent<Renderer>().enabled = false;
        transform.ApplyRecursively(t => {/*Debug.Log("Processing: "+t.name+", test: "+t.name.StartsWith("Vertex"));*/t.gameObject.SetActive(!t.name.StartsWith("Vertex")); }, false);//keep after configuration loading which creates new vertices.
        playMode = true;
        
        Debug.Log("In Play Mode. Projection main texture: " + GetComponent<Renderer>().material.mainTexture + ", screens[1] active: " + screens[1].activeSelf + ", videoColor contains \"" + resourceName + "\" key: " + Settings.videoColor.ContainsKey(resourceName));
        
        Debug.LogWarning(Settings.videoColor.ContainsKey(resourceName)
            ? "\"" + resourceName + "\"'s color \"" + Settings.videoColor[resourceName] + "\" in colorDefaults: " + Settings.colorDefaults.Any(cd => cd.Key == Settings.videoColor[resourceName]) + ". Index: " + Settings.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.videoColor[resourceName]) + "\nColor defaults: " + Settings.colorDefaults.Select(kvp => kvp.Key + ":" + kvp.Value).ToList().Stringify()
            : "\"" + resourceName + "\" not in videoColor.");
        if (Settings.useCueCore) SRSUtilities.TCPMessage(((Settings.videoColor.ContainsKey(resourceName) && Settings.colorDefaults.Any(cd => cd.Key == Settings.videoColor[resourceName]) ? Settings.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.videoColor[resourceName]) : UnityEngine.Random.Range(0, Settings.colorDefaults.Length)) + 1).ToString("D3") + "\n", Settings.cuecoreIP, Settings.cuecorePort);
        Menu.Displayer = Menu.ShowPlayer;
        Menu.limbo = false;
        Settings.ShowCursor(false);

        do
        {
            WWW w = null;
            string fullPath = null;
            foreach (string extension in extensions)
            {
                if (slide > -1 && (!Directory.Exists(Settings.patientDir + SRSUtilities.slashChar + resourceName) || Directory.GetFiles(Settings.patientDir + SRSUtilities.slashChar + resourceName).Length < 1))
                {
                    Debug.Log("\"" + Settings.patientDir + SRSUtilities.slashChar + resourceName + "\" does not exist or is empty; quitting.");
                    yield break;
                }

                fullPath = (slide < 0 ? Settings.libraryDir + SRSUtilities.slashChar + resourceName + (extension.Length > 0 ? "." + extension : "") : (slides = Directory.GetFiles(Settings.patientDir + SRSUtilities.slashChar + resourceName))[slide]);
                Debug.Log("Iteration start. Slide: " + slide + ", fullPath: " + fullPath + ", exists: " + File.Exists(fullPath));
                //fullPath="file://"+Menu.libraryDir+SRSUtilities.slashChar+resourceName+".jpg";
                w = new WWW("file://" + fullPath);
                while (!w.isDone/*&&w.error!=null*/) yield return 0;
                Debug.Log("w.error for " + fullPath + ": " + w.error);
                if (w.error == null) break;
                Debug.Log("No " + extension + " (\"" + w.error + "\"); trying next.");
                slide = -2;
            }
            if (w.error != null)
            {
                Debug.LogError("File \"" + fullPath + "\" fehler: " + w.error);
                yield break;
                //return false;
            }
            if (Regex.IsMatch(fullPath, "\\.ogg$")) slide = -1;//Catch-all for if it ended ogg already.
            if (slide == -1) while (!w.GetMovieTexture().isReadyToPlay) yield return 0;
            Debug.Log("=== numScreens: " + numScreens + ", screenNum: " + screenNum + " === fullPath settled on: \"" + fullPath + "\"");
            if (thisLoop != currentSlideLoop) break;//Catch race condition in case we stopped it while loading.
            for (int i = 0; i < numScreens; i++) if (i == screenNum || screenNum >= numScreens)
            {//{Debug.Log("___ i: "+i+", screenNum: "+screenNum+", numScreens: "+numScreens+", (i==screenNum||screenNum>=numScreens): "+(i==screenNum||screenNum>=numScreens)+", (i==screenNum): "+(i==screenNum)+", (screenNum>=numScreens): "+(screenNum>=numScreens));
                Debug.Log("--Playing \"" + resourceName + "\" on screen " + i + ". (screenNum: " + screenNum + "), numScreens: " + numScreens);
                screens[i].SetActive(true);
                if (Playing(i)) StopMovie(i);
                if (PlayerPrefs.HasKey("DefaultConfiguration-" + i) && File.Exists(PlayerPrefs.GetString("DefaultConfiguration-" + i)))
                {
                    ContourEditor.LoadConfiguration(PlayerPrefs.GetString("DefaultConfiguration-" + i), i);
                    Debug.Log("DefaultConfiguration-" + i + ": " + PlayerPrefs.GetString("DefaultConfiguration-" + i));
                }
                    else
                    {
                        Debug.Log("No saved configuration found for \"DefaultConfiguration-" + i + "\"");
                        if (editing) ContourEditor.Reset(i);
                    }
                    if (slide == -1)
                    {//movie
                        screens[i].GetComponent<Renderer>().material.mainTexture = w.GetMovieTexture();
                        (screens[i].GetComponent<Renderer>().material.mainTexture as MovieTexture).loop = true;
                        (screens[i].GetComponent<Renderer>().material.mainTexture as MovieTexture).Play();
                    }
                    else
                    {//Photo slide
                        screens[i].GetComponent<Renderer>().material.mainTexture = w.texture;
                    }
                    Debug.Log("Playing \"" + resourceName + "\" on screen " + screenNum + " as \"" + screens[i].GetComponent<Renderer>().material.mainTexture.name + "\".");
                    if (slide < 0 && Settings.sound)
                    {
                        Debug.Log("Replacing Sound.");
                        GetComponent<AudioSource>().clip = (screens[i].GetComponent<Renderer>().material.mainTexture as MovieTexture).audioClip;
                    }
                Debug.Log("-" + i + "-NACH pos: " + screens[i].transform.position + ", scale: " + screens[i].transform.localScale + ", bounds: " + screens[i].GetComponent<Renderer>().bounds.size);
            }
            
            Debug.Log("Changing to slide: " + slide);
            if (slide > -1)
            {
                yield return new WaitForSeconds(Settings.slideInterval);
                slide = (slide + 1) % slides.Length;
            }
        } while (slide > -1 && thisLoop == currentSlideLoop);

        Debug.Log("Finished slide " + slide + " routine " + thisLoop + ", current slide loop: " + currentSlideLoop);
        if (Settings.sound)
        {
            Debug.Log("Playing Sound.");
            GetComponent<AudioSource>().loop = true;
            GetComponent<AudioSource>().Play();
            GetComponent<AudioSource>().volume = Settings.volume;
        }
        else Debug.Log("Sound off.");
    }

    public void StopMovie(int screenNum = -1)
    {
        Debug.Log("Projection.StopMovie(" + screenNum + ")");
        if (screenNum < 0 || screenNum >= numScreens)
        {
            for (int i = 0; i < numScreens; i++) StopMovie(i);
            return;
        }

        GameObject screen = screens[screenNum];
        if ((screen.GetComponent<Renderer>().material.mainTexture as MovieTexture) != null)
            (screen.GetComponent<Renderer>().material.mainTexture as MovieTexture).Stop();
        screen.GetComponent<Renderer>().material.mainTexture = Menu.instance.blankScreen;
        if (Settings.sound) GetComponent<AudioSource>().Stop();
    }

    private static void UnitTest()
    {
        Debug.LogWarning("UnitTest (alle wirklich sollen): " + SRSUtilities.Intersect(Vector2.zero, Vector2.one, new Vector2(1, 0), new Vector2(0, 1)) + "," +
                               SRSUtilities.Intersect(new Vector2(0, 0), new Vector2(0, 2), new Vector2(-1, 1), new Vector2(1, 1)) + "," +
                               SRSUtilities.Intersect(new Vector2(1, 0), new Vector2(0, 1), new Vector2(8, 0), new Vector2(0, 1)) + "," +
                               SRSUtilities.Intersect(new Vector2(0, 0), new Vector2(0, 2), new Vector2(1, 1), new Vector2(-1, 1)) + "," +
                               SRSUtilities.Intersect(new Vector2(100, 20), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 10)));
        Debug.LogWarning("UnitTest (alle falch sollen): " + SRSUtilities.Intersect(Vector2.zero, Vector2.one, new Vector2(1, 0), new Vector2(2, 1)) + "," +
                               SRSUtilities.Intersect(new Vector2(0, 0), new Vector2(0, 2), new Vector2(-1, 1), new Vector2(-1, 99)) + "," +
                               SRSUtilities.Intersect(new Vector2(1, 0), new Vector2(0, 1), new Vector2(8, 0), new Vector2(0, 8)) + "," +
                               SRSUtilities.Intersect(new Vector2(0, 0), new Vector2(0, 2), new Vector2(2, 2), new Vector2(2, -2)) + "," +
                               SRSUtilities.Intersect(new Vector2(100, 20), new Vector2(1000, 200), new Vector2(1, 0), new Vector2(1, 10)));
    }

    public static void Rotate(int screenNum = 0)
    {
        Debug.Log("Projection.Rotate(" + screenNum + ")");
        instance.screens[screenNum].transform.Rotate(new Vector3(0, 180, 0));
    }
}