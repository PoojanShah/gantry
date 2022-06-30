using UnityEngine;
using System;
using System.IO;
using Configs;
using Core;
using TMPro;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class MainMenu : MonoBehaviour
	{
		private const string QTS_PATTERN_TITLE = "Selected pattern: ";

		[SerializeField] private Button _settingButton, _exitButton;
		[SerializeField] private Transform _parent;
		[SerializeField] private TMP_Text _currentPatternTitle;

		public void Init(Action<int> playVideoAction, Action onSettingAction, Action onQuitAction, VideosConfig videos, ICommonFactory factory)
		{
			_settingButton.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			InitVideoItems(videos, factory, playVideoAction);

			InitCurrentConfigTitle();
		}

		private void InitCurrentConfigTitle()
		{
			const string defaultConfigKey = "DefaultConfiguration-" + Constants.ZeroString;

			if (!PlayerPrefs.HasKey(defaultConfigKey) || !File.Exists(PlayerPrefs.GetString(defaultConfigKey)))
				return;

			var title = PlayerPrefs.GetString(defaultConfigKey);

			_currentPatternTitle.text = QTS_PATTERN_TITLE + title;
		}

		private void InitVideoItems(VideosConfig config, ICommonFactory commonFactory, Action<int> playVideoAction)
		{
			for (var i = 0; i < config.Videos.Length; i++)
			{
				var videoItem = commonFactory.InstantiateObject<VideoItem>(config.VideoItemPrefab, _parent);
				videoItem.Init(i, playVideoAction, config.Videos[i].name);
			}
		}
	}
}