using UnityEngine;
using System;
using Configs;
using Core;
using Media;
using UnityEngine.UI;

namespace Screens
{
	public class MainMenu : MainMenuBase
	{
		[SerializeField] private Button _settingButton, _muteButton;
		[SerializeField] private Transform _parent;
		[SerializeField] private MediaContentController _contentController;

		private MediaController _mediaController;
		private OptionsSettings _settings;
		private Action<MediaContent> _playVideo;

		public void Init(MediaController mediaController, Action<MediaContent> playVideoAction, Action onSettingAction,
			Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory, OptionsSettings settings)
		{
			_settings = settings;
			_mediaController = mediaController;
			_playVideo = playVideoAction;

			_settingButton?.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_muteButton.onClick.AddListener(SwitchSound);

#if UNITY_STANDALONE
			_contentController.Init(_mediaController, factory, mediaPrefab, PlayMedia);
#endif
			InitVersionTitle();

			SetCurrentOutputType(settings.OutputsNumber == 0
				? OutputTypesConfig.GetSprite(OutputType.Both)
				: OutputTypesConfig.GetSprite(OutputType.Secondary));
		}

		private void PlayMedia(MediaContent content)
		{
			_playVideo?.Invoke(content);
		}

		private void SwitchSound() => _settings.SwitchSound();
		private void OnDestroy() => _settingButton?.onClick.RemoveAllListeners();
	}
}