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
		private MediaController _mediaController;
		private ICommonFactory _factory;
		private GameObject _mediaPrefab;
		private MediaItem[] _mediaItems;
#if UNITY_STANDALONE_WIN
		private Action<MediaContent> _playVideoAction;
#elif UNITY_ANDROID
		private Action<int> _playVideoAction;
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
		public void Init(ICommonFactory factory, GameObject mediaPrefab, Action<int> playVideoAction)
		{
			_playVideoAction = playVideoAction;
			_mediaPrefab = mediaPrefab;
			_factory = factory;

			InitMediaItems();

			_back.onClick.AddListener(ShowPreviousPage);
			_forward.onClick.AddListener(ShowNextPage);

			_currentPage = 0;

			SetButtonInteractable(true, false);
			SetButtonInteractable(false, false);
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

#if UNITY_STANDALONE_WIN
				_mediaItems[i].Init(itemsToShow[i], PlayById);
#elif UNITY_ANDROID
				_mediaItems[i].Init(i, PlayById);
#endif
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
#if UNITY_STANDALONE_WIN
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

			if(finishIndex > _mediaController.MediaFiles.Length)
				finishIndex = _mediaController.MediaFiles.Length;

			var result = new List<MediaContent>();

			for(var i = startIndex; i < finishIndex; i++)
				result.Add(_mediaController.MediaFiles[i]);

			return result.ToArray();
		}

		private void ChangePage(bool isPageIncreased)
		{
			var maxPageNumber = _mediaController.MediaFiles.Length / MEDIA_PER_PAGE;

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
