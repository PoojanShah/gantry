using UnityEngine;
using System;
using System.Collections.Generic;
using Core;
using Media;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class MainMenuAndroid : MonoBehaviour
	{
		[SerializeField] private Button _exitButton;
		[SerializeField] private Transform _parent;

		private List<MediaItem> _mediaItems;
		private MediaController _mediaController;

		public void Init(MediaController mediaController, Action<MediaContent> playVideoAction, Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory)
		{
			_mediaController = mediaController;
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			InitMediaItems(mediaController.MediaFiles, factory, mediaPrefab, playVideoAction);
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
			_exitButton.onClick.RemoveAllListeners();
		}

		public void ClearMediaItems()
		{
			foreach (var mediaItem in _mediaItems)
				Destroy(mediaItem.gameObject);

			_mediaItems.Clear();
		}

		public void InitMediaItems(IEnumerable<MediaContent> media, ICommonFactory commonFactory,
			GameObject mediaPrefab, Action<MediaContent> playVideoAction)
		{
			if (media == null)
				return;

			_mediaItems = new List<MediaItem>();

			foreach (var mediaFile in media)
			{
				var mediaItem = commonFactory.InstantiateObject<MediaItem>(mediaPrefab, _parent);
				mediaItem.Init(mediaFile, playVideoAction, mediaFile.Name);
				mediaItem.SetInteractable(!_mediaController.IsDownloading);

				_mediaItems.Add(mediaItem);
			}
		}
	}
}