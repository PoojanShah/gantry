using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Media;
using UnityEngine;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class MediaContentController : MonoBehaviour
	{
		private const int MEDIA_PER_PAGE = 6;

		[SerializeField] private Button _back, _forward;
		[SerializeField] private Transform _mediaParent;

		private int _currentPage;
		private ICommonFactory _factory;
		private GameObject _mediaPrefab;
		private MediaItem[] _mediaItems;

#if UNITY_STANDALONE_WIN
		private MediaController _mediaController;
		private Action<MediaContent> _playVideoAction;
#elif UNITY_ANDROID
		private Action<int> _playVideoAction;
		private int _mediaAmount;
		private MediaContent[] _media;
#endif

#if UNITY_STANDALONE_WIN
		public void Init(MediaController mediaController, ICommonFactory factory, GameObject mediaPrefab,
			Action<MediaContent> playVideoAction)
		{
			_playVideoAction = playVideoAction;
			_mediaPrefab = mediaPrefab;
			_factory = factory;
			_mediaController = mediaController;

			InitMediaItems();

			DisplayMedia();

			_back.onClick.AddListener(ShowPreviousPage);
			_forward.onClick.AddListener(ShowNextPage);

			_currentPage = 0;

			SetButtonInteractable(true, false);
			SetButtonInteractable(false, _mediaController.MediaFiles.Length > MEDIA_PER_PAGE);
		}
#elif UNITY_ANDROID
		public void Init(ICommonFactory factory, GameObject mediaPrefab, Action<int> playVideoAction, int mediaAmount)
		{
			_playVideoAction = playVideoAction;
			_mediaPrefab = mediaPrefab;
			_factory = factory;
			_mediaAmount = mediaAmount;
			
			_media = new MediaContent[mediaAmount];

			for (var i = 0; i < mediaAmount; i++)
				_media[i] = new MediaContent { Id = i };

			InitMediaItems();

			DisplayMedia();

			_back.onClick.AddListener(ShowPreviousPage);
			_forward.onClick.AddListener(ShowNextPage);

			_currentPage = 0;

			SetButtonInteractable(true, false);
			SetButtonInteractable(false, _media.Length > MEDIA_PER_PAGE);
		}
#endif

		private void InitMediaItems()
		{
			_mediaItems = new MediaItem[MEDIA_PER_PAGE];

			for (var i = 0; i < MEDIA_PER_PAGE; i++)
			{
				var item = _factory.InstantiateObject<MediaItem>(_mediaPrefab, _mediaParent);

				_mediaItems[i] = item;
			}
		}

		private void DisplayMedia()
		{
			var itemsToShow = GetMediaToShow();

			for (var i = 0; i < _mediaItems.Length; i++)
			{
				if (i + 1 > itemsToShow.Length)
				{
					_mediaItems[i].gameObject.SetActive(false);

					continue;
				}

				_mediaItems[i].Init(itemsToShow[i], PlayById);
				_mediaItems[i].SetInteractable(true);
				_mediaItems[i].gameObject.SetActive(true);
			}
		}

		public void SetButtonInteractable(bool isBackButton, bool isInteractable)
		{
			if(isBackButton)
				_back.interactable = isInteractable;
			else 
				_forward.interactable = isInteractable;
		}

		public void PlayById(int id)
		{
#if UNITY_STANDALONE
			var media = _mediaController.MediaFiles.First(mf => mf.Id == id);

			_playVideoAction?.Invoke(media);
#elif UNITY_ANDROID
			_playVideoAction?.Invoke(id);
#endif
		}

		private void ShowNextPage() => ChangePage(true);
		private void ShowPreviousPage() => ChangePage(false);

		private MediaContent[] GetMediaToShow()
		{
			var startIndex = _currentPage * MEDIA_PER_PAGE;
			var finishIndex = startIndex + MEDIA_PER_PAGE;
			var result = new List<MediaContent>();

#if UNITY_STANDALONE
			if(finishIndex > _mediaController.MediaFiles.Length)
				finishIndex = _mediaController.MediaFiles.Length;

			for (var i = startIndex; i < finishIndex; i++)
				result.Add(_mediaController.MediaFiles[i]);
#elif UNITY_ANDROID
			if (finishIndex > _media.Length)
				finishIndex = _media.Length;

			for (var i = startIndex; i < finishIndex; i++)
				result.Add(_media[i]);
#endif
			return result.ToArray();
		}

		private void ChangePage(bool isPageIncreased)
		{
#if UNITY_STANDALONE
			var maxPageNumber = _mediaController.MediaFiles.Length / MEDIA_PER_PAGE;
#elif UNITY_ANDROID
			var maxPageNumber = _media.Length / MEDIA_PER_PAGE;
#endif

			if (isPageIncreased)
				_currentPage++;
			else
				_currentPage--;

			_currentPage = Mathf.Clamp(_currentPage, 0, maxPageNumber);

			DisplayMedia();

			SetButtonInteractable(true, _currentPage > 0);
			SetButtonInteractable(false, _currentPage != maxPageNumber);
		}
	}
}