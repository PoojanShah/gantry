using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Configs;
using Core;
using Media;
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

		public void Init(MediaContent[] media, Action<MediaContent> playVideoAction, Action onSettingAction, Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory)
		{
			_settingButton.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			InitMediaItems(media, factory, mediaPrefab, playVideoAction);

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

		private void InitMediaItems(IReadOnlyList<MediaContent> media, ICommonFactory commonFactory,
			GameObject mediaPrefab, Action<MediaContent> playVideoAction)
		{
			for (var i = 0; i < media.Count; i++)
			{
				var videoItem = commonFactory.InstantiateObject<MediaItem>(mediaPrefab, _parent);
				videoItem.Init(media[i], playVideoAction, media[i].Name);
			}
		}
	}
}