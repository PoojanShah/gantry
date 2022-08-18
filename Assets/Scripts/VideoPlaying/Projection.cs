using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Linq;
using ContourEditorTool;
using ContourToolsAndUtilities;
using Core;
using Media;
using Screens;

namespace VideoPlaying
{
	public class Projection : MonoBehaviour
	{
		public VideoPlayerScreen[] Screens => _screens;

		[SerializeField] private VideoPlayerScreen[] _screens;
		[SerializeField] private Renderer _renderer;

		public bool IsEditing;

		public static Vector3 originalExtents;

		[SerializeField] private ContourEditor _contourEditor;
		private OptionsSettings _settings;

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

		public void Init(OptionsSettings settings)
		{
			_settings = settings;

			GetComponent<MeshFilter>().mesh.Clear();

			const float extentsFactor = 5;

			ApplyRotation();

			transform.localScale = new Vector3(Settings.originalScaleX, 1, 1);
			originalExtents = Vector3.one * extentsFactor;

			var scale = new Vector3(Settings.ScreenWidth / (float)Settings.ScreenHeight, 1.0f, 1.0f);

			transform.localScale = scale;

			for (var i = 0; i < _screens.Length; i++)
			{
				_screens[i].Transform.localScale = scale;
				_screens[i].Transform.position = ScreenPosition(i);
			}
		}

		public void ApplyRotation()
		{
			const float rotationSetting = 180.0f;

			foreach (var videoPlayerScreen in _screens)
			{
				videoPlayerScreen.Transform.localRotation = !_settings.IsRotationOn
					? Quaternion.Euler(0.0f, 0.0f, 0.0f)
					: Quaternion.Euler(0.0f, rotationSetting, 0.0f);
			}
		}

		public Vector3 ScreenPosition(int screenNum)
		{
			Debug.Log("Projection.ScreenPosition(" + screenNum + "), DisplaysAmount: " + DisplaysAmount + ", original extents: " + Projection.originalExtents.x +
				", local scale: " + transform.localScale + ", ergebnis: " + new Vector3(originalExtents.x * transform.localScale.x * (-0.5f * ((float)(DisplaysAmount - 1)) + screenNum), 0, 0));
			return new Vector3(originalExtents.x * transform.localScale.x * (-1 * ((float)(DisplaysAmount - 1)) + screenNum * 2), 0, 0);
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
			var settingsKey = mediaToPlay.Name;
			var selectedColor = Settings.VideoColors[settingsKey];

			var colorKeyValue = Constants.colorDefaults
				.FirstOrDefault(cd => cd.Key == selectedColor);
			var color32 = colorKeyValue.Value;

			CameraHelper.SetBackgroundColor(color32);

			IsEditing = false;

			if (IsScreenPlayingById(screenNum)) 
				StopMovie(screenNum);

			const int cameraHeight = 10;
			CameraHelper.SetCameraPosition(Vector3.zero + Vector3.up * cameraHeight);
			gameObject.SetActive(true);

			StopCoroutine("LoadAndPlayExternalResource");
			StartCoroutine(LoadAndPlayExternalResource(mediaToPlay, screenNum));
			GetComponent<Toolbar>().enabled = false;
			Debug.Log("_screens.Length: " + _screens.Length + ", screen 2 not null: " + (_screens[1] != null) + ", _screens[0].transform.width: " + _screens[0].Transform.localScale.x);
		}

		private IEnumerator LoadAndPlayExternalResource(MediaContent content, int screenNum = 0)
		{
			gameObject.SetActive(true);
			enabled = false;
			_renderer.enabled = false;

			IsPlayMode = true;
			//if (Settings.useCueCore)
			//	SRSUtilities.TCPMessage(
			//		((Settings.videoColor.ContainsKey(content.Name) &&
			//		  Constants.colorDefaults.Any(cd => cd.Key == Settings.videoColor[content.Name])
			//			? Constants.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.videoColor[content.Name])
			//			: UnityEngine.Random.Range(0, Constants.colorDefaults.Length)) + 1).ToString("D3") + "\n",
			//		Settings.cuecoreIP, Settings.cuecorePort);

			Settings.ShowCursor(false);

			yield return new WaitForEndOfFrame();

			for (var i = 0; i < DisplaysAmount; i++)
			{
				if (i != screenNum && screenNum < DisplaysAmount) 
					continue;

				_screens[i].SetActive(true);

				if (IsScreenPlayingById(i))
					StopMovie(i);
				if (PlayerPrefs.HasKey(Constants.DefaultConfigHash) &&
				    File.Exists(PlayerPrefs.GetString(Constants.DefaultConfigHash)))
				{
					_contourEditor.LoadConfiguration(PlayerPrefs.GetString(Constants.DefaultConfigHash), i);
				}
				else
				{
					Debug.Log("No saved configuration found for " + Constants.DefaultConfigHash);

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

				SaveCurrentVideoPlaying(true, content);
			}

			const float showBlackoutsDelay = 0.1f;
			yield return new WaitForSeconds(showBlackoutsDelay);

			GetComponent<ContourEditor>().enabled = true;
		}

		public void SaveCurrentVideoPlaying(bool isSave, MediaContent mediaContent = null)
		{
			PlayerPrefs.SetString(Constants.LastPlayedMediaHash, isSave ? mediaContent.Path : string.Empty);
			PlayerPrefs.Save();
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

			GetComponent<ContourEditor>().enabled = false;
		}

		public void Clear()
		{
			SaveCurrentVideoPlaying(false);

			Destroy(_renderer.sharedMaterial.mainTexture);
		}
	}
}