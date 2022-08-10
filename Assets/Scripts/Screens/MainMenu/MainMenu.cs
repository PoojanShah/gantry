using UnityEngine;
using System;
using System.IO;
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

		[SerializeField] private Button _settingButton;
		[SerializeField] private Transform _parent;
		[SerializeField] private TMP_Text _currentPatternTitle, _serverIpTitle;
		[SerializeField] private MediaContentController _contentController;

		private MediaController _mediaController;

		public void Init(MediaController mediaController, Action<MediaContent> playVideoAction, Action onSettingAction,
			Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory)
		{
			_mediaController = mediaController;
			_settingButton?.onClick.AddListener(() => { onSettingAction?.Invoke(); });

#if UNITY_STANDALONE_WIN
			_contentController.Init(_mediaController, factory, mediaPrefab, playVideoAction);
#endif
			InitCurrentConfigTitle();

			_serverIpTitle.text = QTS_IP_TITLE + NetworkHelper.GetMyIp();
		}

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