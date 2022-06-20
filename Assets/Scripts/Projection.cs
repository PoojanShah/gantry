using UnityEngine;//NOTE: the up-side-down phenomenon occurs when you haven't loaded a gantry configuration.
using System;
using System.IO;
using System.Collections;
using System.Linq;
using Configs;
using ContourEditorTool;
using UnityEngine.Serialization;
using UnityEngine.Video;

public class Projection : MonoBehaviour
{
    [SerializeField] private GameObject[] _screens;
    [SerializeField] private VideosConfig _videosConfig;

    public GameObject[] Screens => _screens;

    public bool IsEditing
    {
	    get => _contourEditor != null && _contourEditor.enabled;
	    set { _contourEditor.enabled = value; }
    }

    public static Vector3 originalExtents, rawSize = new Vector3(5, 0, 5);
    [FormerlySerializedAs("editor")][SerializeField]private ContourEditor _contourEditor;
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
                _screens[i].SetActive(value);
            }

            GetComponent<Renderer>().enabled = !value;
        }
    }

    public bool playing
    {
        get
        {
            return gameObject.activeSelf && !IsEditing;
        }
    }

    public string playingMovieName
    {
        get
        {
            Debug.Log("Removed code");

            //foreach (var screen in _screens)
            //{
            //    if (screen.activeSelf && (screen.GetComponent<Renderer>().material.mainTexture as VideoPlayer) != null)
            //    {
            //        return (screen.GetComponent<Renderer>().material.mainTexture as VideoPlayer).name;
            //    }
            //}

            return "";
        }
    }

    public Vector3 ScreenPosition(int screenNum)
    {
        Debug.Log("Projection.ScreenPosition(" + screenNum + "), numScreens: " + numScreens + ", original extents: " + Projection.originalExtents.x +
            ", local scale: " + transform.localScale + ", ergebnis: " + new Vector3(originalExtents.x * transform.localScale.x * (-0.5f * ((float) (numScreens - 1)) + screenNum), 0, 0));
        return new Vector3(originalExtents.x * transform.localScale.x * (-1 * ((float) (numScreens - 1)) + screenNum * 2), 0, 0);
    }

    public static void StopAllMovies()
    {
	    Debug.Log("Removed code");

        //for (int i = 0; i < instance._screens.Length; i++)
        //{
        //    MovieTexture mt = instance._screens[i].GetComponent<Renderer>().material.mainTexture as MovieTexture;
        //    if (mt != null) mt.Stop();
        //}
    }

    public bool Playing(int screenNum)
    {
        return screenNum < 0 || screenNum > _screens.Length - 1 ? _screens.All<GameObject>(o => { return Playing(o); }) : Playing(_screens[screenNum]);
    }

    public bool Playing(GameObject screenObj)
    {
        return screenObj != null && screenObj.activeSelf && screenObj.GetComponent<Renderer>().material.mainTexture != null;
    }

    public string PlayingMovieName(int screenNum)
    {
        Debug.Log("Removed code");
        return string.Empty;
        //return Playing(screenNum) ? (_screens[0].GetComponent<Renderer>().material.mainTexture as MovieTexture).name : "";
    }

    private void Awake()
    {
        Debug.Log("Projection.Awake(); Settings.dataPath: " + Settings.dataPath + ", Application.persistentDataPath: " + Application.persistentDataPath
            + ";\nCommand Line: \"" + Environment.CommandLine + "\", Command Line Args: \"" + string.Join(",", Environment.GetCommandLineArgs()) + "\"; Screen.width: " + Screen.width + ", Screen.height: " + Screen.height + ", numScreens: " + numScreens);

        GetComponent<MeshFilter>().mesh.Clear();
        transform.localScale = new Vector3(Settings.originalScaleX, 1, 1);
        originalExtents = Vector3.one * 5;
        Debug.Log("originalExtents: " + Projection.originalExtents);
        transform.localScale = new Vector3(4f / 3f, 1, 1);

        for (var i = 0; i < _screens.Length; i++)
        {
            _screens[i].transform.localScale = new Vector3(4f / 3f, 1, 1);
            _screens[i].transform.position = ScreenPosition(i);
        }

        Debug.Log("Original Scale X: " + Settings.originalScaleX + ", Screen.width: " + Screen.width + ", screen 0 width: " + _screens[0].transform.localScale.x + " (Screen.width/1024)=" + (Screen.width / 1024));
        Debug.Log("originalExtents: " + originalExtents);
        if (numScreens > 1)
        {
            Debug.Log("Screen.width: " + Screen.width + ", screen 0 width: " + _screens[0].transform.localScale.x);
        }
        Debug.Log("Original Scale X 2: " + Settings.originalScaleX + ", Screen.width: " + Screen.width + ", screen 0 width: " + _screens[0].transform.localScale.x + " (Screen.width/1024)=" + (Screen.width / 1024));
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

        for (int i = 0; i < _screens.Length; i++)
        {
            GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "S" + i + " pos: " + _screens[i].transform.position.x);
            _screens[i].transform.position = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
                _screens[i].transform.position.x, 2, -10, 10), _screens[i].transform.position.y, _screens[i].transform.position.z);
            GUI.Label(new Rect(Screen.width * 0.5f - 128, ypos * 24, 128, 24), "S" + i + " scale: " + _screens[i].transform.localScale.x);
            _screens[i].transform.localScale = new Vector3(GUI.HorizontalScrollbar(new Rect(Screen.width * 0.5f, (ypos++) * 24, Screen.width * 0.5f, 24),
                _screens[i].transform.localScale.x, 2, -10, 10), _screens[i].transform.localScale.y, _screens[i].transform.localScale.z);
        }
    }
    
    public void StartMovie(int screenNum = 0, bool testMovie = false)
    {
	    var clip = _videosConfig.GetFirstClip();

        Debug.Log("Projection.StartMovie(\"" + clip.name + "\"," + screenNum + "," + testMovie + "); timeScale: " + Time.timeScale);
        IsEditing = false;
        if (Playing(screenNum)) StopMovie(screenNum);
        Camera.main.transform.position = Vector3.zero + Vector3.up * 5;
        gameObject.SetActive(true);
        
        StopCoroutine("LoadAndPlayExternalResource");
        StartCoroutine(LoadAndPlayExternalResource(clip, screenNum));
        GetComponent<Toolbar>().enabled = GetComponent<InfoDisplay>().enabled = false;
        Debug.Log("_screens.Length: " + _screens.Length + ", screen 2 not null: " + (_screens[1] != null) + ", _screens[0].transform.width: " + _screens[0].transform.localScale.x);
    }

    public void StartSlideshow(string spritePath, int screenNum = 0)
    {
        Debug.Log("Removed code");
        //Debug.Log("Projection.StartMovie(\"" + sprite.name + "\"," + screenNum + "); timeScale: " + Time.timeScale);
        //IsEditing = false;
        //if (Playing(screenNum)) StopMovie(screenNum);
        //Camera.main.transform.position = Vector3.zero + Vector3.up * 5;
        //gameObject.SetActive(true);
        
        //instance.StopCoroutine("LoadAndPlayExternalResource");
        //instance.StartCoroutine(LoadAndPlayExternalResource(clip, screenNum, 0));
        //instance.GetComponent<Toolbar>().enabled = instance.GetComponent<InfoDisplay>().enabled = false;
        //Debug.Log("_screens.Length: " + _screens.Length + ", screen 2 not null: " + (_screens[1] != null) + ", _screens[0].transform.width: " + _screens[0].transform.localScale.x);
    }

    private IEnumerator LoadAndPlayExternalResource(VideoClip clip, int screenNum = 0, int slide = -1)
    {
	    //Slide of -1 is a movie; resourceName is the ogg file name if movie, patient folder name if slide.
	    //stopSlides=true;//slide<0;//Clever reliance on the fact that an existing loop will finish in less time than this one, unless they happened to click this on the exact frame of it in which case it'll be equal.
	    int thisLoop = ++currentSlideLoop;
	    string[] slides = null, extensions = { "ogg", "jpg", "png", "" };
	    Debug.Log("Projection.LoadAndPlayExternalResource(\"" + clip.name + "\"," + screenNum + "," + slide + ");");
	    Menu.instance.menuBackground.SetActive(false);
	    gameObject.SetActive(true);
	    enabled = false;
	    GetComponent<Renderer>().enabled = false;
	    transform.ApplyRecursively(t =>
	    {
		    /*Debug.Log("Processing: "+t.name+", test: "+t.name.StartsWith("Vertex"));*/
		    t.gameObject.SetActive(!t.name.StartsWith("Vertex"));
	    }, false); //keep after configuration loading which creates new vertices.
	    playMode = true;

	    Debug.Log("In Play Mode. Projection main texture: " + GetComponent<Renderer>().material.mainTexture +
	              ", _screens[1] active: " + _screens[1].activeSelf + ", videoColor contains \"" + clip.name +
	              "\" key: " + Settings.videoColor.ContainsKey(clip.name));

	    Debug.LogWarning(Settings.videoColor.ContainsKey(clip.name)
		    ? "\"" + clip.name + "\"'s color \"" + Settings.videoColor[clip.name] + "\" in colorDefaults: " +
		      Settings.colorDefaults.Any(cd => cd.Key == Settings.videoColor[clip.name]) + ". Index: " +
		      Settings.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.videoColor[clip.name]) +
		      "\nColor defaults: " +
		      Settings.colorDefaults.Select(kvp => kvp.Key + ":" + kvp.Value).ToList().Stringify()
		    : "\"" + clip.name + "\" not in videoColor.");
	    if (Settings.useCueCore)
		    SRSUtilities.TCPMessage(
			    ((Settings.videoColor.ContainsKey(clip.name) &&
			      Settings.colorDefaults.Any(cd => cd.Key == Settings.videoColor[clip.name])
				    ? Settings.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.videoColor[clip.name])
				    : UnityEngine.Random.Range(0, Settings.colorDefaults.Length)) + 1).ToString("D3") + "\n",
			    Settings.cuecoreIP, Settings.cuecorePort);
	    Menu.Displayer = Menu.ShowPlayer;
	    Menu.limbo = false;
	    Settings.ShowCursor(false);

	    do
	    {
//            UnityWebRequest request = null;
//            string fullPath = null;
//            foreach (string extension in extensions)
//            {
//                if (slide > -1 && (!Directory.Exists(Settings.patientDir + SRSUtilities.slashChar + clip.name) || Directory.GetFiles(Settings.patientDir + SRSUtilities.slashChar + resourceName).Length < 1))
//                {
//                    Debug.Log("\"" + Settings.patientDir + SRSUtilities.slashChar + clip.name + "\" does not exist or is empty; quitting.");
//                    yield break;
//                }
//#if UNITY_EDITOR
//	            fullPath = Application.dataPath + "/Videos/Abstract_Girl.mp4";
//#endif
//                //fullPath = (slide < 0 ? Settings.libraryDir + SRSUtilities.slashChar + resourceName + (extension.Length > 0 ? "." + extension : "") : (slides = Directory.GetFiles(Settings.patientDir + SRSUtilities.slashChar + resourceName))[slide]);
//                Debug.Log("Iteration start. Slide: " + slide + ", fullPath: " + fullPath + ", exists: " + File.Exists(fullPath));
//                //fullPath="file://"+Menu.libraryDir+SRSUtilities.slashChar+resourceName+".jpg";
//                request = new UnityWebRequest("file://" + fullPath);
//                while (!request.isDone/*&&w.error!=null*/) yield return 0;
//                Debug.Log("w.error for " + fullPath + ": " + request.error);
//                if (request.error == null) break;
//                Debug.Log("No " + extension + " (\"" + request.error + "\"); trying next.");
//                slide = -2;
//            }
//            if (request.error != null)
//            {
//                Debug.LogError("File \"" + fullPath + "\" fehler: " + request.error);
//                yield break;
//                //return false;
//            }
//            if (Regex.IsMatch(fullPath, "\\.ogg$")) slide = -1;//Catch-all for if it ended ogg already.
		    Debug.Log("Removed code");
		    yield return new WaitForEndOfFrame();
		    //if (slide == -1) while (!w.GetMovieTexture().isReadyToPlay) yield return 0;
		    Debug.Log("=== numScreens: " + numScreens + ", screenNum: " + screenNum + " === fullPath settled on: \"" +
		              clip.name + "\"");
		    if (thisLoop != currentSlideLoop) break; //Catch race condition in case we stopped it while loading.
		    for (int i = 0; i < numScreens; i++)
			    if (i == screenNum || screenNum >= numScreens)
			    {
				    //{Debug.Log("___ i: "+i+", screenNum: "+screenNum+", numScreens: "+numScreens+", (i==screenNum||screenNum>=numScreens): "+(i==screenNum||screenNum>=numScreens)+", (i==screenNum): "+(i==screenNum)+", (screenNum>=numScreens): "+(screenNum>=numScreens));
				    Debug.Log("--Playing \"" + clip.name + "\" on screen " + i + ". (screenNum: " + screenNum +
				              "), numScreens: " + numScreens);
				    _screens[i].SetActive(true);
				    if (Playing(i)) StopMovie(i);
				    if (PlayerPrefs.HasKey("DefaultConfiguration-" + i) &&
				        File.Exists(PlayerPrefs.GetString("DefaultConfiguration-" + i)))
				    {
					    _contourEditor.LoadConfiguration(PlayerPrefs.GetString("DefaultConfiguration-" + i), i);
					    Debug.Log("DefaultConfiguration-" + i + ": " +
					              PlayerPrefs.GetString("DefaultConfiguration-" + i));
				    }
				    else
				    {
					    Debug.Log("No saved configuration found for \"DefaultConfiguration-" + i + "\"");
					    if (IsEditing) _contourEditor.Reset(i);
				    }

				    if (slide == -1)
				    {
					    //movie
                        var player = _screens[i].GetComponent<VideoPlayer>();
                        player.clip = clip;
                        player.isLooping = true;
                        player.Play();
				    }
				    else
				    {
                        Debug.Log("Photo");
					    //Photo slide
					    //_screens[i].GetComponent<Renderer>().material.mainTexture = request.texture;
				    }

				    Debug.Log("Playing \"" + clip.name + "\" on screen " + screenNum + " as \"" +
				              _screens[i].GetComponent<Renderer>().material.mainTexture.name + "\".");
				    if (slide < 0 && Settings.sound)
				    {
					    Debug.Log("Replacing Sound.");
					    Debug.Log("Removed code");

					    //GetComponent<AudioSource>().clip = (_screens[i].GetComponent<Renderer>().material.mainTexture as VideoPlayer).audioClip;
				    }

				    Debug.Log("-" + i + "-NACH pos: " + _screens[i].transform.position + ", scale: " +
				              _screens[i].transform.localScale + ", bounds: " +
				              _screens[i].GetComponent<Renderer>().bounds.size);
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

        GameObject screen = _screens[screenNum];
        Debug.Log("Removed code");

        //if ((screen.GetComponent<Renderer>().material.mainTexture as MovieTexture) != null)
        //    (screen.GetComponent<Renderer>().material.mainTexture as MovieTexture).Stop();
        screen.GetComponent<Renderer>().material.mainTexture = Menu.instance.blankScreen;
        if (Settings.sound) GetComponent<AudioSource>().Stop();
    }

    public void Rotate(int screenNum = 0)
    {
        Debug.Log("Projection.Rotate(" + screenNum + ")");
        
        _screens[screenNum].transform.Rotate(new Vector3(0, 180, 0));
    }
}