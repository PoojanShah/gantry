using System;
using Configs;
using ContourEditorTool;
using Media;
using UnityEngine;
using UnityEngine.Video;

namespace VideoPlaying
{
	public class ProjectionView : MonoBehaviour
	{
		[SerializeField] private VideoPlayer[] _players;
		[SerializeField] private Projection _projection;

		private Action _stopAction;

		public void Init(Action stopAction)
		{
			_stopAction = stopAction;

			_projection.Init();
		}

		public void Play(MediaContent content) => _projection.StartMovie(content);
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

			_projection.IsPlayMode = false;
			_projection.Clear();

			foreach (var videoPlayer in _players)
			{
				if (videoPlayer.isPlaying)
					videoPlayer.Stop();
			}

			Settings.ShowCursor();

			_stopAction?.Invoke();
		}
	}
}
