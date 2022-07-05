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
		private readonly MediaConfig _mediaConfig;
		private readonly Action _stopAction;

		private ProjectionView _projectionView;

		public ProjectionController(ICommonFactory commonFactory, GameObject prefab, MediaConfig mediaConfig,
			Action stopAction)
		{
			_commonFactory = commonFactory;
			_prefab = prefab;
			_mediaConfig = mediaConfig;
			_stopAction = stopAction;
		}

		public Projection GetProjection()
		{
			if(_projectionView == null)
				CreateProjectionView();

			return _projectionView.GetProjection();
		}

		public void Play(int videoId)
		{
			if (_projectionView != null)
			{
				_projectionView.SetActive(true);
				_projectionView.Init(_mediaConfig, StopAndHidePlayer);
				_projectionView.Play(videoId);

				return;
			}

			CreateProjectionView();

			_projectionView.Play(videoId);
		}

		private void CreateProjectionView()
		{
			_projectionView = _commonFactory.InstantiateObject<ProjectionView>(_prefab);
			_projectionView.Init(_mediaConfig, StopAndHidePlayer);
		}

		private void StopAndHidePlayer()
		{
			_projectionView.SetActive(false);

			_stopAction?.Invoke();
		}
	}
}
