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
		[SerializeField] private TMP_Text _currentPatternTitle, _debug;

		public void Init(Action<int> playVideoAction, Action onSettingAction, Action onQuitAction, MediaConfig media, ICommonFactory factory)
		{
			_settingButton.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			InitMediaItems(media, factory, playVideoAction);

			InitCurrentConfigTitle();

			_debug.text = Directory.GetParent(Application.dataPath).ToString();
		}

		private void InitCurrentConfigTitle()
		{
			const string defaultConfigKey = "DefaultConfiguration-" + Constants.ZeroString;

			if (!PlayerPrefs.HasKey(defaultConfigKey) || !File.Exists(PlayerPrefs.GetString(defaultConfigKey)))
				return;

			var title = PlayerPrefs.GetString(defaultConfigKey);

			_currentPatternTitle.text = QTS_PATTERN_TITLE + title;
		}

		private void InitMediaItems(MediaConfig config, ICommonFactory commonFactory, Action<int> playVideoAction)
		{
			for (var i = 0; i < config.MediaFiles.Length; i++)
			{
				var videoItem = commonFactory.InstantiateObject<MediaItem>(config.MediaItemPrefab, _parent);
				videoItem.Init(i, playVideoAction, config.MediaFiles[i].Name);
			}
		}
	}
}