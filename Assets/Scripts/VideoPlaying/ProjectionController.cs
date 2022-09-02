using System;
using Core;
using Media;
using Screens;
using UnityEngine;

namespace VideoPlaying
{
	public class ProjectionController
	{
		private readonly ICommonFactory _commonFactory;
		private readonly GameObject _prefab;
		private readonly Action _stopAction;
		private readonly OptionsSettings _optionsSettings;

		private ProjectionView _projectionView;

		public ProjectionController(ICommonFactory commonFactory, GameObject prefab, Action stopAction,
			OptionsSettings optionsSettings)
		{
			_commonFactory = commonFactory;
			_prefab = prefab;
			_stopAction = stopAction;
			_optionsSettings = optionsSettings;
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
			}
			else
				CreateProjectionView();

			_projectionView.ApplyRotation();

			SetSoundSettings();

			CameraHelper.SetMainCameraActive(false);

			_projectionView.Play(content);
		}

		private void CreateProjectionView()
		{
			_projectionView = _commonFactory.InstantiateObject<ProjectionView>(_prefab);
			_projectionView.Init(StopAndHidePlayer, _optionsSettings, _commonFactory);
			_projectionView.ApplyRotation();
		}

		private void StopAndHidePlayer()
		{
			_projectionView.SetActive(false);

			_stopAction?.Invoke();

			CameraHelper.SetMainCameraActive(true);
		}

		private void SetSoundSettings() => _projectionView.SetSoundSettings(_optionsSettings.IsSoundOn);
	}
}
