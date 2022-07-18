using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
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
		[SerializeField] private TMP_Text _currentPatternTitle;

		private List<MediaItem> _mediaItems;

		public void Init(MediaController mediaController, Action<MediaContent> playVideoAction, Action onSettingAction, Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory)
		{
			_settingButton.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			InitMediaItems(mediaController.MediaFiles, factory, mediaPrefab, playVideoAction);

			InitCurrentConfigTitle();
		}

		public void ClearMediaItems()
		{
			foreach (var mediaItem in _mediaItems)
				Destroy(mediaItem.gameObject);

			_mediaItems.Clear();
		}

		private void InitCurrentConfigTitle()
		{
			const string defaultConfigKey = "DefaultConfiguration-" + Constants.ZeroString;

			if (!PlayerPrefs.HasKey(defaultConfigKey) || !File.Exists(PlayerPrefs.GetString(defaultConfigKey)))
				return;

			var title = PlayerPrefs.GetString(defaultConfigKey);

			_currentPatternTitle.text = QTS_PATTERN_TITLE + title;
		}

		public void InitMediaItems(IEnumerable<MediaContent> media, ICommonFactory commonFactory,
			GameObject mediaPrefab, Action<MediaContent> playVideoAction)
		{
			_mediaItems = new List<MediaItem>();

			foreach (var mediaFile in media)
			{
				var mediaItem = commonFactory.InstantiateObject<MediaItem>(mediaPrefab, _parent);
				mediaItem.Init(mediaFile, playVideoAction, mediaFile.Name);

				_mediaItems.Add(mediaItem);
			}
		}
	}
}