using UnityEngine;//NOTE: the up-side-down phenomenon occurs when you haven't loaded a gantry configuration.
using System;
using System.IO;
using System.Collections;
using System.Linq;
using Configs;
using ContourEditorTool;
using UnityEngine.Video;

namespace VideoPlaying
{
	[Serializable]
	public class VideoPlayerScreen
	{
		public Transform Transform;
		public VideoPlayer Player;

		[SerializeField] private GameObject _gameObject;

		public GameObject GetObject() => _gameObject;
		public bool IsActive() => _gameObject.activeSelf;
		public void SetActive(bool isActive) => _gameObject.SetActive(isActive);
	}

	public class Projection : MonoBehaviour
	{
		[SerializeField] private VideoPlayerScreen[] _screens;
		[SerializeField] private VideosConfig _videosConfig;
		[SerializeField] private Renderer _renderer;

		public VideoPlayerScreen[] Screens => _screens;

		public bool IsEditing
		{
			get => _contourEditor != null && _contourEditor.enabled;
			set => _contourEditor.enabled = value;
		}

		public static Vector3 originalExtents;

		[SerializeField] private ContourEditor _contourEditor;

		public static int currentSlideLoop = 0;

		public static int DisplaysAmount
		{
			get
			{
#if UNITY_EDITOR
				return 1;
#else
				return Display.displays.Length;
#endif
			}
		}

		public bool IsPlayMode
		{
			get => IsPlaying;
			set
			{
				for (var i = 0; i < DisplaysAmount; i++)
					_screens[i].SetActive(value);

				_renderer.enabled = !value;
			}
		}

		public bool IsPlaying => gameObject.activeSelf && !IsEditing;

		private void Awake()
		{
			Debug.Log("Projection.Awake(); Settings.dataPath: " + Settings.dataPath + ", Application.persistentDataPath: " + Application.persistentDataPath
					  + ";\nCommand Line: \"" + Environment.CommandLine + "\", Command Line Args: \"" + string.Join(",", Environment.GetCommandLineArgs()) + "\"; Screen.width: " + Screen.width + ", Screen.height: " + Screen.height + ", DisplaysAmount: " + DisplaysAmount);

			GetComponent<MeshFilter>().mesh.Clear();
			transform.localScale = new Vector3(Settings.originalScaleX, 1, 1);
			originalExtents = Vector3.one * 5;
			Debug.Log("originalExtents: " + Projection.originalExtents);
			transform.localScale = new Vector3(4f / 3f, 1, 1);

			for (var i = 0; i < _screens.Length; i++)
			{
				_screens[i].Transform.localScale = new Vector3(4f / 3f, 1, 1);
				_screens[i].Transform.position = ScreenPosition(i);
			}

			Debug.Log("Original Scale X: " + Settings.originalScaleX + ", Screen.width: " + Screen.width + ", screen 0 width: " + _screens[0].Transform.localScale.x + " (Screen.width/1024)=" + (Screen.width / 1024));
			Debug.Log("originalExtents: " + originalExtents);
			if (DisplaysAmount > 1)
			{
				Debug.Log("Screen.width: " + Screen.width + ", screen 0 width: " + _screens[0].Transform.localScale.x);
			}
			Debug.Log("Original Scale X 2: " + Settings.originalScaleX + ", Screen.width: " + Screen.width + ", screen 0 width: " + _screens[0].Transform.localScale.x + " (Screen.width/1024)=" + (Screen.width / 1024));
		}

		public Vector3 ScreenPosition(int screenNum)
		{
			Debug.Log("Projection.ScreenPosition(" + screenNum + "), DisplaysAmount: " + DisplaysAmount + ", original extents: " + Projection.originalExtents.x +
				", local scale: " + transform.localScale + ", ergebnis: " + new Vector3(originalExtents.x * transform.localScale.x * (-0.5f * ((float)(DisplaysAmount - 1)) + screenNum), 0, 0));
			return new Vector3(originalExtents.x * transform.localScale.x * (-1 * ((float)(DisplaysAmount - 1)) + screenNum * 2), 0, 0);
		}

		public void StopAllScreens()
		{
			foreach (var screen in Screens)
				screen.Player.Stop();
		}

		public bool IsScreenPlayingById(int screenNum)
		{
			return screenNum < 0 || screenNum > _screens.Length - 1
				? _screens.All(IsScreenPlaying)
				: IsScreenPlaying(_screens[screenNum]);
		}

		public bool IsScreenPlaying(VideoPlayerScreen playerScreen) =>
			playerScreen != null && playerScreen.IsActive() && playerScreen.Player.isPlaying;

		public void StartMovie(int screenNum = 0, bool testMovie = false)
		{
			var clip = _videosConfig.GetFirstClip();

			Debug.Log("Projection.StartMovie(\"" + clip.name + "\"," + screenNum + "," + testMovie + "); timeScale: " + Time.timeScale);
			IsEditing = false;
			if (IsScreenPlayingById(screenNum)) StopMovie(screenNum);
			Camera.main.transform.position = Vector3.zero + Vector3.up * 5;
			gameObject.SetActive(true);

			StopCoroutine("LoadAndPlayExternalResource");
			StartCoroutine(LoadAndPlayExternalResource(clip, screenNum));
			GetComponent<Toolbar>().enabled = GetComponent<InfoDisplay>().enabled = false;
			Debug.Log("_screens.Length: " + _screens.Length + ", screen 2 not null: " + (_screens[1] != null) + ", _screens[0].transform.width: " + _screens[0].Transform.localScale.x);
		}

		private IEnumerator LoadAndPlayExternalResource(VideoClip clip, int screenNum = 0, int slide = -1)
		{
			//Slide of -1 is a movie; resourceName is the ogg file name if movie, patient folder name if slide.
			//stopSlides=true;//slide<0;//Clever reliance on the fact that an existing loop will finish in less time than this one, unless they happened to click this on the exact frame of it in which case it'll be equal.
			int thisLoop = ++currentSlideLoop;
			string[] slides = null, extensions = { "ogg", "jpg", "png", "" };
			Debug.Log("Projection.LoadAndPlayExternalResource(\"" + clip.name + "\"," + screenNum + "," + slide + ");");
			gameObject.SetActive(true);
			enabled = false;
			_renderer.enabled = false;
			transform.ApplyRecursively(t =>
			{
			/*Debug.Log("Processing: "+t.name+", test: "+t.name.StartsWith("Vertex"));*/
				t.gameObject.SetActive(!t.name.StartsWith("Vertex"));
			}, false); //keep after configuration loading which creates new vertices.
			IsPlayMode = true;

			Debug.Log("In Play Mode. Projection main texture: " + _renderer.material.mainTexture +
					  ", _screens[1] active: " + _screens[1].IsActive() + ", videoColor contains \"" + clip.name +
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
			//Menu.Displayer = Menu.ShowPlayer;
			Menu.limbo = false;
			Settings.ShowCursor(false);

			do
			{
				yield return new WaitForEndOfFrame();

				if (thisLoop != currentSlideLoop)
					break; //Catch race condition in case we stopped it while loading.

				for (int i = 0; i < DisplaysAmount; i++)
					if (i == screenNum || screenNum >= DisplaysAmount)
					{
						//{Debug.Log("___ i: "+i+", displayId: "+displayId+", DisplaysAmount: "+DisplaysAmount+", (i==displayId||displayId>=DisplaysAmount): "+(i==displayId||displayId>=DisplaysAmount)+", (i==displayId): "+(i==displayId)+", (displayId>=DisplaysAmount): "+(displayId>=DisplaysAmount));
						Debug.Log("--Playing \"" + clip.name + "\" on screen " + i + ". (displayId: " + screenNum +
								  "), DisplaysAmount: " + DisplaysAmount);
						_screens[i].SetActive(true);
						if (IsScreenPlayingById(i)) StopMovie(i);
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
							var player = _screens[i].Player;
							player.clip = clip;
							player.isLooping = true;
							player.Play();
						}
						else
						{
							Debug.Log("Photo");
							//Photo slide
							//_screens[i]._renderer.material.mainTexture = request.texture;
						}
					}

				Debug.Log("Changing to slide: " + slide);
				if (slide > -1)
				{
					yield return new WaitForSeconds(Settings.slideInterval);
					slide = (slide + 1) % slides.Length;
				}
			} while (slide > -1 && thisLoop == currentSlideLoop);

			Debug.Log("Finished slide " + slide + " routine " + thisLoop + ", current slide loop: " + currentSlideLoop);
		}

		public void StopMovie(int screenNum = -1)
		{
			if (screenNum < 0 || screenNum >= DisplaysAmount)
			{
				for (var i = 0; i < DisplaysAmount; i++)
					StopMovie(i);

				return;
			}

			var screen = _screens[screenNum];
			screen.Player.Stop();
		}

		public void Rotate(int displayId = 0)
		{
			const float rotateAmount = 180.0f;

			_screens[displayId].Transform.Rotate(new Vector3(0, rotateAmount, 0));
		}
	}
}