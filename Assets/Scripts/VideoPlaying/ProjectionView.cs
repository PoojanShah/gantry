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

		public void Play() => _projection.StartMovie();
		public void SetActive(bool isActive) => gameObject.SetActive(isActive);
		private void Update() => InputHandler();

		private void InputHandler()
		{
			if (!_projection.IsPlaying || !Input.GetKeyDown(KeyCode.Escape))
				return;

			ContourEditor.WipeBlackouts();

			_projection.IsPlayMode = false;

			foreach (var videoPlayer in _players)
			{
				if(videoPlayer.isPlaying)
					videoPlayer.Stop();
			}

			Settings.ShowCursor();

			_stopAction?.Invoke();
		}
	}
}
