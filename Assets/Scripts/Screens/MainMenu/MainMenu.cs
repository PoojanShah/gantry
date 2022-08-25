using UnityEngine;
using System;
using System.IO;
using System.Linq;
using Core;
using Media;
using Network;
using TMPro;
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

		public void Init(MediaController mediaController, Action<MediaContent> playVideoAction, Action onSettingAction,
			Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory, OptionsSettings settings)
		{
			_settings = settings;
			_mediaController = mediaController;

			_settingButton?.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_muteButton.onClick.AddListener(SwitchSound);

#if UNITY_STANDALONE
			_contentController.Init(_mediaController, factory, mediaPrefab, playVideoAction);
#endif
			InitVersionTitle();

			CheckCrash(playVideoAction);
		}

		private void CheckCrash(Action<MediaContent> playVideoAction)
		{
			if(!PlayerPrefs.HasKey(Constants.LastPlayedMediaHash))
				return;

			var mediaName = Path.GetFileName(PlayerPrefs.GetString(Constants.LastPlayedMediaHash));

			if(string.IsNullOrEmpty(mediaName))
				return;

			var media = _mediaController.MediaFiles.FirstOrDefault(m => m.Name == mediaName);

			if(media == null)
				return;

			playVideoAction?.Invoke(media);
		}

		private void SwitchSound() => _settings.SwitchSound();
		private void OnDestroy() => _settingButton?.onClick.RemoveAllListeners();
	}
}