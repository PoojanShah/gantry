using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using Configs;
using ContourEditorTool;
using Core;
using Media;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace VideoPlaying
{
	[Serializable]
	public class VideoPlayerScreen
	{
		public Transform Transform;
		public VideoPlayer Player;

		[SerializeField] private GameObject _gameObject;
		[SerializeField] private Renderer _renderer;

		public GameObject GetObject() => _gameObject;
		public bool IsActive() => _gameObject.activeSelf;
		public void SetActive(bool isActive) => _gameObject.SetActive(isActive);
		public void SetTexture(Texture texture) => _renderer.sharedMaterial.mainTexture = texture;

		public void Stop()
		{
			Player.Stop();
			Player.clip = null;

			_renderer.sharedMaterial.mainTexture = null;
		}
	}

	public class Projection : MonoBehaviour
	{
		public VideoPlayerScreen[] Screens => _screens;

		[SerializeField] private VideoPlayerScreen[] _screens;
		[SerializeField] private Renderer _renderer;

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

		public void Init()
		{
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

		public void StartMovie(MediaContent mediaToPlay, int screenNum = 0, bool testMovie = false)
		{
			CameraHelper.SetBackgroundColor(Constants.colorDefaults
				.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.mediaLibrary[int.Parse(mediaToPlay.Name)]])
				.Value);

			IsEditing = false;
			if (IsScreenPlayingById(screenNum)) StopMovie(screenNum);
			CameraHelper.SetCameraPosition(Vector3.zero + Vector3.up * 5);
			gameObject.SetActive(true);

			StopCoroutine("LoadAndPlayExternalResource");
			StartCoroutine(LoadAndPlayExternalResource(mediaToPlay, screenNum));
			GetComponent<Toolbar>().enabled = GetComponent<InfoDisplay>().enabled = false;
			Debug.Log("_screens.Length: " + _screens.Length + ", screen 2 not null: " + (_screens[1] != null) + ", _screens[0].transform.width: " + _screens[0].Transform.localScale.x);
		}

		private IEnumerator LoadAndPlayExternalResource(MediaContent content, int screenNum = 0, int slide = -1)
		{
			//Slide of -1 is a movie; resourceName is the ogg file name if movie, patient folder name if slide.
			//stopSlides=true;//slide<0;//Clever reliance on the fact that an existing loop will finish in less time than this one, unless they happened to click this on the exact frame of it in which case it'll be equal.
			int thisLoop = ++currentSlideLoop;
			string[] slides = null, extensions = { "ogg", "jpg", "png", "" };
			Debug.Log("Projection.LoadAndPlayExternalResource(\"" + content.Name + "\"," + screenNum + "," + slide + ");");
			gameObject.SetActive(true);
			enabled = false;
			_renderer.enabled = false;
			transform.ApplyRecursively(t =>
			{
			/*Debug.Log("Processing: "+t.name+", test: "+t.name.StartsWith("Vertex"));*/
				t.gameObject.SetActive(!t.name.StartsWith("Vertex"));
			}, false); //keep after configuration loading which creates new vertices.
			IsPlayMode = true;

			//if (Settings.useCueCore)
			//	SRSUtilities.TCPMessage(
			//		((Settings.videoColor.ContainsKey(content.Name) &&
			//		  Constants.colorDefaults.Any(cd => cd.Key == Settings.videoColor[content.Name])
			//			? Constants.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.videoColor[content.Name])
			//			: UnityEngine.Random.Range(0, Constants.colorDefaults.Length)) + 1).ToString("D3") + "\n",
			//		Settings.cuecoreIP, Settings.cuecorePort);

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
						Debug.Log("--Playing \"" + content.Name + "\" on screen " + i + ". (displayId: " + screenNum +
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

							if (IsEditing)
								_contourEditor.Reset(i);
						}

						if (content.IsVideo)
						{
							var player = _screens[i].Player;
							player.url = content.Path;
							player.isLooping = true;
							player.Play();
						}
						else
						{
							var loadImageFromFile = MediaController.LoadImageFromFile(content.Path);
							_screens[i].Player.Stop();
							_screens[i].SetTexture(loadImageFromFile);
						}
					}

				if (slide > -1)
				{
					yield return new WaitForSeconds(Settings.slideInterval);
					slide = (slide + 1) % slides.Length;
				}
			} while (slide > -1 && thisLoop == currentSlideLoop);
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
			screen.Stop();
		}

		public void Rotate(int displayId = 0)
		{
			const float rotateAmount = 180.0f;

			_screens[displayId].Transform.Rotate(new Vector3(0, rotateAmount, 0));
		}
	}
}