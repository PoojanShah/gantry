using System;
using Core;
using Media;
using UnityEngine;

namespace VideoPlaying
{
	public class ProjectionController
	{
		private readonly ICommonFactory _commonFactory;
		private readonly GameObject _prefab;
		private readonly Action _stopAction;

		private ProjectionView _projectionView;

		public ProjectionController(ICommonFactory commonFactory, GameObject prefab, Action stopAction)
		{
			_commonFactory = commonFactory;
			_prefab = prefab;
			_stopAction = stopAction;
		}

		public Projection GetProjection()
		{
			if(_projectionView == null)
				CreateProjectionView();

			return _projectionView.GetProjection();
		}

		public void Play(MediaContent content)
		{
			if (_projectionView != null)
			{
				_projectionView.SetActive(true);
				_projectionView.Init(StopAndHidePlayer);
				_projectionView.Play(content);

				return;
			}

			CreateProjectionView();

			_projectionView.Play(content);
		}

		private void CreateProjectionView()
		{
			_projectionView = _commonFactory.InstantiateObject<ProjectionView>(_prefab);
			_projectionView.Init(StopAndHidePlayer);
		}

		private void StopAndHidePlayer()
		{
			_projectionView.SetActive(false);

			_stopAction?.Invoke();
		}
	}
}
