using UnityEngine;
using System.IO;
using System.Collections;
using System.Linq;
using ContourEditorTool;
using ContourToolsAndUtilities;
using Core;
using Media;
using Screens;
using UnityEngine.Video;

namespace VideoPlaying
{
	public class Projection : MonoBehaviour
	{
		public ProjectionOutputView[] OutputViews { get; set; }

		public static Vector3 originalExtents;
		public bool IsEditing;

		[SerializeField] private Renderer _renderer;
		[SerializeField] private ContourEditor _contourEditor;

		private OptionsSettings _settings;

#if !UNITY_EDITOR
		public static int DisplaysAmount => Display.displays.Length;
#elif UNITY_EDITOR
		public static int DisplaysAmount => 3;
#endif


		public bool IsPlayMode
		{
			get => IsPlaying;
			set
			{
				for (var i = 0; i < DisplaysAmount; i++)
					OutputViews[i].SetActive(value);

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

			for (var i = 0; i < OutputViews.Length; i++)
			{
				OutputViews[i].Transform.localScale = scale;
				OutputViews[i].Transform.position = ScreenPosition(i);
			}
		}

		public void ApplyRotation() => OutputViews[0].ApplyRotation(_settings.IsRotationOn);

		public Vector3 ScreenPosition(int screenNum)
		{
			Debug.Log("Projection.ScreenPosition(" + screenNum + "), DisplaysAmount: " + DisplaysAmount + ", original extents: " + Projection.originalExtents.x +
				", local scale: " + transform.localScale + ", ergebnis: " + new Vector3(originalExtents.x * transform.localScale.x * (-0.5f * ((float)(DisplaysAmount - 1)) + screenNum), 0, 0));
			return new Vector3(originalExtents.x * transform.localScale.x * (-1 * ((float)(DisplaysAmount - 1)) + screenNum * 2), 0, 0);
		}

		public bool IsScreenPlaying(ProjectionOutputView playerScreen) =>
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

			const int cameraHeight = 10;
			CameraHelper.SetCameraPosition(Vector3.zero + Vector3.up * cameraHeight);
			gameObject.SetActive(true);

			StopCoroutine("LoadAndPlayExternalResource");
			StartCoroutine(LoadAndPlayExternalResource(mediaToPlay, screenNum));
			GetComponent<Toolbar>().enabled = false;
		}

		public void SetSoundSettings(bool enableAudio)
		{
			foreach (var screen in OutputViews)
				screen.Player.SetDirectAudioMute(0, !enableAudio);
		}

		private IEnumerator LoadAndPlayExternalResource(MediaContent content, int screenNum = 0)
		{
			gameObject.SetActive(true);
			enabled = false;
			_renderer.enabled = false;

			IsPlayMode = true;

			if (_settings.IsCueCoreEnabled)
				SRSUtilities.TCPMessage(
					((Settings.VideoColors.ContainsKey(content.Name) &&
					  Constants.colorDefaults.Any(cd => cd.Key == Settings.VideoColors[content.Name])
						? Constants.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.VideoColors[content.Name])
						: UnityEngine.Random.Range(0, Constants.colorDefaults.Length)) + 1).ToString("D3") + "\n",
					_settings.CuoCoreIp, _settings.CuoCorePort);

			Settings.ShowCursor(false);

			yield return new WaitForEndOfFrame();

			for (var i = 0; i < DisplaysAmount; i++)
			{
				if (i != screenNum && screenNum < DisplaysAmount) 
					continue;

				OutputViews[i].SetActive(true);

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

				void PlayVideo(VideoPlayer player)
				{
					player.url = content.Path;
					player.isLooping = true;
					player.Play();
				}

				void ShowImage(Texture texture, ProjectionOutputView output) => output.SetTexture(texture);

				if (content.IsVideo)
				{
					PlayVideo(OutputViews[i].Player);

					for (var j = 1; j < DisplaysAmount; j++)
						PlayVideo(OutputViews[j].Player);
				}
				else
				{
					StopMovies();

					var loadedImage = MediaController.LoadImageFromFile(content.Path);

					ShowImage(loadedImage, OutputViews[0]);

					for (var j = 1; j < DisplaysAmount; j++) 
						ShowImage(loadedImage, OutputViews[j]);
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

		public void StopMovies()
		{
			foreach (var output in OutputViews)
				output.Stop();

			GetComponent<ContourEditor>().enabled = false;
		}

		public void Clear()
		{
			SaveCurrentVideoPlaying(false);

			Destroy(_renderer.sharedMaterial.mainTexture);
		}
	}
}