using UnityEngine;
using System.IO;
using System.Collections;
using System.Linq;
using Configs;
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
		public static int DisplaysAmount => 2;
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

		public void StartMovie(MediaContent mediaToPlay, OutputType output = OutputType.Both)
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
			StartCoroutine(LoadAndPlayExternalResource(mediaToPlay, output));
			GetComponent<Toolbar>().enabled = false;
		}

		public void SetSoundSettings(bool enableAudio, OutputType outputType)
		{
			foreach (var screen in OutputViews)
				screen.Player.SetDirectAudioMute(0, true);

			if (outputType < OutputType.Secondary)
				OutputViews[0].Player.SetDirectAudioMute(0, !enableAudio);
			
			for (var i = 1; i < OutputViews.Length; i++)
			{
				var screen = OutputViews[i];
				screen.Player.SetDirectAudioMute(0, true);
			}
		}

		private IEnumerator LoadAndPlayExternalResource(MediaContent content, OutputType output = OutputType.Both)
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

			if (output == OutputType.Primary)
				Settings.ShowCursor(false);

			yield return new WaitForEndOfFrame();

			if (PlayerPrefs.HasKey(Constants.DefaultConfigHash) &&
			    File.Exists(PlayerPrefs.GetString(Constants.DefaultConfigHash)))
			{
				_contourEditor.LoadConfiguration(PlayerPrefs.GetString(Constants.DefaultConfigHash), 0);
			}
			else
			{
				Debug.Log("No saved configuration found for " + Constants.DefaultConfigHash);

				if (IsEditing)
					_contourEditor.Reset(0);
			}

			void PlayVideo(ProjectionOutputView output)
			{
				output.Player.url = content.Path;
				output.Player.isLooping = true;
				output.Player.Play();
			}

			void ShowImage(Texture texture, ProjectionOutputView output) => output.SetTexture(texture);

			if (content.IsVideo)
			{
				if (output < OutputType.Secondary)
				{
					PlayVideo(OutputViews[0]);

					OutputViews[0].SetActive(true);

					SaveCurrentVideoPlaying(true, true, content);

				}
				else if (output != OutputType.Primary)
				{
					for (var j = 1; j < DisplaysAmount; j++)
					{
						PlayVideo(OutputViews[j]);

						OutputViews[j].SetActive(true);
					}

					SaveCurrentVideoPlaying(true, false, content);
				}
			}
			else
			{
				var loadedImage = MediaController.LoadImageFromFile(content.Path);

				if (output < OutputType.Secondary)
				{
					OutputViews[0].SetActive(true);

					ShowImage(loadedImage, OutputViews[0]);

					SaveCurrentVideoPlaying(true, true, content);
				}
				else if (output != OutputType.Primary)
				{
					for (var j = 1; j < DisplaysAmount; j++)
					{
						ShowImage(loadedImage, OutputViews[j]);

						OutputViews[j].SetActive(true);
					}

					SaveCurrentVideoPlaying(true, false, content);
				}
			}

			const float showBlackoutsDelay = 0.1f;

			yield return new WaitForSeconds(showBlackoutsDelay);

			if (output != OutputType.Secondary)
				GetComponent<ContourEditor>().enabled = true;
		}

		public void SaveCurrentVideoPlaying(bool isSave, bool isPrimary, MediaContent mediaContent = null)
		{
			PlayerPrefs.SetString(
				isPrimary ? Constants.LastPlayedPrimaryMediaHash : Constants.LastPlayedSecondaryMediaHash,
				isSave ? mediaContent.Path : string.Empty);
			PlayerPrefs.Save();
		}

		public void StopMovies()
		{
			foreach (var output in OutputViews)
				output.StopVideo();

			GetComponent<ContourEditor>().enabled = false;
		}

		public void Clear()
		{
			SaveCurrentVideoPlaying(false, false);
			SaveCurrentVideoPlaying(false, true);

			Destroy(_renderer.sharedMaterial.mainTexture);
		}
	}
}