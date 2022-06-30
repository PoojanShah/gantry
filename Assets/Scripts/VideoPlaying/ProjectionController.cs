using System;
using Configs;
using Core;
using UnityEngine;

namespace VideoPlaying
{
	public class ProjectionController
	{
		private readonly ICommonFactory _commonFactory;
		private readonly GameObject _prefab;
		private readonly VideosConfig _videosConfig;
		private readonly Action _stopAction;

		private ProjectionView _projectionView;

		public ProjectionController(ICommonFactory commonFactory, GameObject prefab, VideosConfig videosConfig,
			Action stopAction)
		{
			_commonFactory = commonFactory;
			_prefab = prefab;
			_videosConfig = videosConfig;
			_stopAction = stopAction;
		}

		public Projection GetProjection() => _projectionView.GetProjection();

		public void Play(int videoId)
		{
			if (_projectionView != null)
			{
				_projectionView.SetActive(true);
				_projectionView.Init(_videosConfig, StopAndHidePlayer);
				_projectionView.Play(videoId);

				return;
			}

			_projectionView = _commonFactory.InstantiateObject<ProjectionView>(_prefab);
			_projectionView.Init(_videosConfig, StopAndHidePlayer);
			_projectionView.Play(videoId);
		}

		private void StopAndHidePlayer()
		{
			_projectionView.SetActive(false);

			_stopAction?.Invoke();
		}
	}
}
