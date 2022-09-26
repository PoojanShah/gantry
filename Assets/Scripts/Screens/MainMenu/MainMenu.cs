using UnityEngine;
using System;
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
		[SerializeField] private GameObject _duoOutput, _singleOutput;

		private MediaController _mediaController;
		private OptionsSettings _settings;
		private Action<MediaContent> _playVideo;

		public void Init(MediaController mediaController, Action<MediaContent> playVideoAction, Action onSettingAction,
			Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory, OptionsSettings settings)
		{
			_settings = settings;
			_mediaController = mediaController;
			_playVideo = playVideoAction;

			_mediaController.OnDownloadCompleted += RefreshMedia;

			_settingButton?.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_muteButton.onClick.AddListener(SwitchSound);

#if UNITY_STANDALONE
			_contentController.Init(_mediaController, factory, mediaPrefab, PlayMedia);
#endif
			InitVersionTitle();

			SetCurrentOutputType(settings.IsDuoOutput);
		}

		private void SetCurrentOutputType(bool isDuo)
		{
			_duoOutput.SetActive(isDuo);
			_singleOutput.SetActive(!isDuo);
		}

		public void RefreshMedia() => _contentController.UpdateMediaItems();
		private void PlayMedia(MediaContent content) => _playVideo?.Invoke(content);
		private void SwitchSound() => _settings.SwitchSound();
		
		private void OnDestroy()
		{ 
			_settingButton?.onClick.RemoveAllListeners();
			_mediaController.OnDownloadCompleted -= RefreshMedia;
		}
	}
}