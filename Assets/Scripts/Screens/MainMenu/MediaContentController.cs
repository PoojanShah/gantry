using System.Collections.Generic;
using Core;
using Media;
using UnityEngine;
using UnityEngine.UI;
using VideoPlaying;

namespace Assets.Scripts.Screens
{
	public class MediaContentController : MonoBehaviour
	{
		private const int MEDIA_PER_PAGE = 6;

		[SerializeField] private Button _back, _forward;
		[SerializeField] private Transform _mediaParent;

		private int _currentPage;
		private MediaContent[] _media;
		private ICommonFactory _factory;
		private GameObject _mediaPrefab;
		private MediaItem[] _mediaItems;

		public void Init(MediaContent[] media, ICommonFactory factory, GameObject mediaPrefab)
		{
			_mediaPrefab = mediaPrefab;
			_factory = factory;
			_media = media;

			InitMediaItems();

			_back.onClick.AddListener(ShowPreviousPage);
			_forward.onClick.AddListener(ShowNextPage);

			_currentPage = 0;

			SetButtonInteractable(true, false);
			SetButtonInteractable(false, _media.Length > MEDIA_PER_PAGE);
		}

		private void InitMediaItems()
		{
			_mediaItems = new MediaItem[MEDIA_PER_PAGE];

			for (var i = 0; i < MEDIA_PER_PAGE; i++)
			{
				var item = _factory.InstantiateObject<MediaItem>(_mediaPrefab, _mediaParent);
				item.gameObject.SetActive(false);

				_mediaItems[i] = item;
			}
		}

		private void DisplayMedia()
		{
			var itemsToShow = GetMediaToShow();

			for (var i = 0; i < _mediaItems.Length; i++)
			{
				if (i > itemsToShow.Length)
				{
					_mediaItems[i].gameObject.SetActive(false);

					continue;
				}
			}
		}

		public void SetButtonInteractable(bool isBackButton, bool isInteractable)
		{
			if(isBackButton)
				_back.interactable = isInteractable;
			else 
				_forward.interactable = isInteractable;
		}

		private void ShowNextPage() => ChangePage(true);
		private void ShowPreviousPage() => ChangePage(false);

		private MediaContent[] GetMediaToShow()
		{
			var startIndex = (_currentPage + 1) * MEDIA_PER_PAGE;
			var result = new List<MediaContent>();

			for(var i = startIndex; i < _media.Length; i++)
				result.Add(_media[i]);

			return result.ToArray();
		}

		private void ChangePage(bool isPageIncreased)
		{
			var maxPageNumber = _media.Length / MEDIA_PER_PAGE;

			Debug.Log(maxPageNumber);

			if (isPageIncreased)
				_currentPage++;
			else
				_currentPage--;

			_currentPage = Mathf.Clamp(_currentPage, 0, maxPageNumber);

			Debug.Log(_currentPage);

			DisplayMedia();
		}
	}
}
