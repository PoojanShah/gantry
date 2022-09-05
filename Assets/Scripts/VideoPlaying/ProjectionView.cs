using System;
using Configs;
using ContourEditorTool;
using Core;
using Media;
using Screens;
using UnityEngine;

namespace VideoPlaying
{
	public class ProjectionView : MonoBehaviour
	{
		[SerializeField] private GameObject _outputPrefab;
		[SerializeField] private Projection _projection;

		private Action _stopAction;
		private ICommonFactory _factory;

		public void Init(Action stopAction, OptionsSettings optionsSettings, ICommonFactory factory)
		{
			_factory = factory;
			_stopAction = stopAction;

			InitOutputPanels();

			_projection.Init(optionsSettings);
		}

		private void InitOutputPanels()
		{
			var screensAmount = Projection.DisplaysAmount;
			var outputViews = new ProjectionOutputView[screensAmount];

			for (var i = 0; i < screensAmount; i++)
			{
				outputViews[i] = _factory.InstantiateObject<ProjectionOutputView>(_outputPrefab, transform);
				outputViews[i].Init(i);
			}

			_projection.OutputViews = outputViews;
		}

		public void SetSoundSettings(bool enableAudio) => _projection.SetSoundSettings(enableAudio);

		public void Play(MediaContent content, OutputType output = OutputType.Both)
		{
			ContourEditor.HideGUI = false;

			_projection.StartMovie(content, output);

#if !UNITY_EDITOR
			for(var i = 1; i < Projection.DisplaysAmount; i++)
				Display.displays[i].Activate();
#endif
		}

		public void SetActive(bool isActive) => gameObject.SetActive(isActive);
		public Projection GetProjection() => _projection;
		private void Update() => InputHandler();

		private void InputHandler()
		{
			if (!_projection.IsPlaying)
				return;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			if(!Input.GetKeyDown(KeyCode.Escape))
				return;
#elif UNITY_ANDROID
			if(Input.touchCount == 0)
				return;
#endif

			StopVideoPlaying();
		}

		private void StopVideoPlaying()
		{
			ContourEditor.WipeBlackouts();
			ContourEditor.ClearLassos();

			_projection.StopMovies();
			_projection.IsPlayMode = false;
			_projection.Clear();

			Settings.ShowCursor();

			_stopAction?.Invoke();
		}

		public void ApplyRotation() => _projection.ApplyRotation();
	}
}
