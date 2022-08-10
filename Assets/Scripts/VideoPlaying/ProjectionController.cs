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

		private ProjectionView _projectionView;
		private OptionsSettings _optionsSettings;

		public ProjectionController(ICommonFactory commonFactory, GameObject prefab, Action stopAction, OptionsSettings optionsSettings)
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
				_projectionView.Init(StopAndHidePlayer);
			}
			else
				CreateProjectionView();

			SetSoundSettings();
			
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

		private void SetSoundSettings()
		{
			_optionsSettings.Load();
			_projectionView.SetSoundSettings(_optionsSettings.IsSoundOn);
		}
	}
}
