using System;
using UnityEngine;
using System.Collections.Generic;
using Core;
using Network;
using UnityEngine.UI;

namespace Screens
{
	public sealed class MainMenuAndroid : MainMenuBase
	{
		[SerializeField] private MediaContentController _contentController;
		[SerializeField] private Button _settingsButton, _muteButton;
		private GameObject _mediaPrefab;
		private ICommonFactory _factory;

		public void Init(GameObject mediaPrefab, ICommonFactory factory, Action showServerPopup)
		{
			_mediaPrefab = mediaPrefab;
			_factory = factory;

			_settingsButton.onClick.AddListener(() =>
			{
				SetUiBlocker(true);

				showServerPopup?.Invoke();
			});

			_muteButton.onClick.AddListener(LocalNetworkClient.SendMuteMessage);

			InitVersionTitle();

			LocalNetworkClient.OnMediaInfoReceived += OnMediaInfoReceived;
			LocalNetworkClient.OnThumbnailReceived += OnThumbnailReceived;

			_contentController.OnThumbnailsLoaded += AllThumbnailsLoaded;
		}
		private void OnMediaInfoReceived(Dictionary<int, string> dictionary)
		{
#if UNITY_ANDROID
			_contentController.ClearMediaItems();
			_contentController.Init(_factory, _mediaPrefab, LocalNetworkClient.SendPlayMessage, dictionary);
#endif
		}

		private void OnThumbnailReceived(Texture2D texture)
		{
#if UNITY_ANDROID
			_contentController.SetThumbnail(texture);
#endif
		}

		private void AllThumbnailsLoaded()
		{
			SetUiBlocker(false);

			Debug.Log("all thumbnails loaded");
		}

		private void OnDestroy()
		{
			LocalNetworkClient.OnMediaInfoReceived -= OnMediaInfoReceived;
			LocalNetworkClient.OnThumbnailReceived -= OnThumbnailReceived;
			_contentController.OnThumbnailsLoaded -= AllThumbnailsLoaded;

			_settingsButton.onClick.RemoveAllListeners();
			_muteButton.onClick.RemoveAllListeners();
		}
	}
}