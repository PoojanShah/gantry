using System;
using Configs;
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
		public OutputType OutputType { private set; get; }
		public string CurrentPlayingMediaName { get; private set; } = string.Empty;

		private ProjectionView _projectionView;

		public ProjectionController(ICommonFactory commonFactory, GameObject prefab, Action stopAction,
			OptionsSettings optionsSettings)
		{
			_commonFactory = commonFactory;
			_prefab = prefab;
			_stopAction = stopAction;
			_optionsSettings = optionsSettings;

			InitOutputType();
		}

		public void InitOutputType()
		{
			if (Projection.DisplaysAmount == 1)
				OutputType = OutputType.Both;
			else
				OutputType = _optionsSettings.IsDuoOutput ? OutputType.Secondary : OutputType.Both;
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

			switch (OutputType)
			{
				case OutputType.Both:
					CameraHelper.SetMainCameraActive(false);

					_projectionView.Play(content);

					SetSoundSettings(OutputType.Both);
					break;
				case OutputType.Secondary:
					_projectionView.Play(content, OutputType.Secondary);

					SetSoundSettings(OutputType.Secondary);

					OutputType = OutputType.Primary;
					break;
				default:
					CameraHelper.SetMainCameraActive(false);

					_projectionView.Play(content, OutputType.Primary);

					SetSoundSettings(OutputType.Primary);
					break;
			}

			CurrentPlayingMediaName = content.Name;
			Debug.Log(CurrentPlayingMediaName);
		}

		private void CreateProjectionView()
		{
			_projectionView = _commonFactory.InstantiateObject<ProjectionView>(_prefab);
			_projectionView.Init(StopAndHidePlayer, _optionsSettings, _commonFactory);
			_projectionView.ApplyRotation();
		}

		public void StopAndHidePlayer()
		{
			_projectionView.SetActive(false);

			_stopAction?.Invoke();

			CameraHelper.SetMainCameraActive(true);
		}

		private void SetSoundSettings(OutputType output) =>
			_projectionView.SetSoundSettings(_optionsSettings.IsSoundOn, output);
	}
}
