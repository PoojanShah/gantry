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
	public class MainMenu : MonoBehaviour
	{
		private const string QTS_PATTERN_TITLE = "Selected pattern: ";
		private const string QTS_IP_TITLE = "Server IP: ";
		private const string QTS_VERSION_PREFIX = "v";

		[SerializeField] private Button _settingButton, _muteButton;
		[SerializeField] private Transform _parent;
		[SerializeField] private TMP_Text _currentPatternTitle, _serverIpTitle, _versionTitle;
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
			InitCurrentConfigTitle();

			InitIpTitle();

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

		private void InitVersionTitle() => _versionTitle.text = QTS_VERSION_PREFIX + Application.version;
		private void InitIpTitle() => _serverIpTitle.text = QTS_IP_TITLE + NetworkHelper.GetMyIp();
		private void SwitchSound() => _settings.SwitchSound();
		private void OnDestroy() => _settingButton?.onClick.RemoveAllListeners();

		private void InitCurrentConfigTitle()
		{
			const string defaultConfigKey = Constants.DefaultConfigHash;

			if (!PlayerPrefs.HasKey(defaultConfigKey) || !File.Exists(PlayerPrefs.GetString(defaultConfigKey)))
				return;

			var title = PlayerPrefs.GetString(defaultConfigKey);

			if(_currentPatternTitle)
				_currentPatternTitle.text = QTS_PATTERN_TITLE + Path.GetFileNameWithoutExtension(title);
		}
	}
}