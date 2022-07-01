using System;
using Configs;
using ContourEditorTool;
using UnityEngine;
using UnityEngine.Video;

namespace VideoPlaying
{
	public class ProjectionView : MonoBehaviour
	{
		[SerializeField] private VideoPlayer[] _players;
		[SerializeField] private Projection _projection;

		private Action _stopAction;

		public void Init(VideosConfig videosConfig, Action stopAction)
		{
			_stopAction = stopAction;

			_projection.Init(videosConfig);
		}

		public void Play(int videoId) => _projection.StartMovie(videoId);
		public void SetActive(bool isActive) => gameObject.SetActive(isActive);
		public Projection GetProjection() => _projection;
		private void Update() => InputHandler();

		private void InputHandler()
		{
			if (!_projection.IsPlaying)
				return;

#if UNITY_EDITOR
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
