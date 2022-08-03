using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Media;
using Network;
using TMPro;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class MainMenu : MonoBehaviour
	{
		private const string QTS_PATTERN_TITLE = "Selected pattern: ";
		private const string QTS_IP_TITLE = "Server IP: ";

		[SerializeField] private Button _settingButton, _exitButton;
		[SerializeField] private Transform _parent;
		[SerializeField] private TMP_Text _currentPatternTitle, _serverIpTitle;

		private List<MediaItem> _mediaItems;
		private MediaController _mediaController;

		public void Init(MediaController mediaController, Action<MediaContent> playVideoAction, Action onSettingAction,
			Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory)
		{
			_mediaController = mediaController;
			_settingButton?.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			if (_mediaController != null)
				InitMediaItems(_mediaController.MediaFiles, factory, mediaPrefab, playVideoAction);

			InitCurrentConfigTitle();

			_serverIpTitle.text = QTS_IP_TITLE + NetworkHelper.GetMyIp();
		}

		public void SetMediaInteractable()
		{
			if(_mediaItems == null || _mediaItems.Count < 1)
				return;

			foreach (var mediaItem in _mediaItems)
				mediaItem.SetInteractable(true);
		}

		private void OnDestroy()
		{
			_settingButton?.onClick.RemoveAllListeners();
			_exitButton.onClick.RemoveAllListeners();
		}

		public void ClearMediaItems()
		{
			foreach (var mediaItem in _mediaItems)
				Destroy(mediaItem.gameObject);

			_mediaItems.Clear();
		}

		private void InitCurrentConfigTitle()
		{
			const string defaultConfigKey = Constants.DefaultConfigHash;

			if (!PlayerPrefs.HasKey(defaultConfigKey) || !File.Exists(PlayerPrefs.GetString(defaultConfigKey)))
				return;

			var title = PlayerPrefs.GetString(defaultConfigKey);

			if(_currentPatternTitle)
				_currentPatternTitle.text = QTS_PATTERN_TITLE + Path.GetFileNameWithoutExtension(title);
		}

		public void InitMediaItems(MediaContent[] media, ICommonFactory commonFactory,
			GameObject mediaPrefab, Action<MediaContent> playVideoAction)
		{
#if UNITY_STANDALONE_WIN
			if (media == null)
				return;

			_mediaItems = new List<MediaItem>();

			for (var i = 0; i < media.Length; i++)
			{
				var mediaFile = media[i];
				var mediaItem = commonFactory.InstantiateObject<MediaItem>(mediaPrefab, _parent);
				mediaItem.Init(mediaFile, playVideoAction, i);
				mediaItem.SetInteractable(!_mediaController.IsDownloading);

				_mediaItems.Add(mediaItem);
			}

		}

		public void PlayById(int id)
		{
			var media = _mediaItems.Find(m => m.Id == id);

			if(media == null)
				return;

			media.ItemClicked();
#endif
		}
	}
}