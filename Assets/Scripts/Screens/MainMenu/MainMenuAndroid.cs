using System;
using UnityEngine;
using System.Collections.Generic;
using Core;
using Network;
using TMPro;
using UnityEngine.UI;

namespace Screens
{
	public sealed class MainMenuAndroid : MainMenuBase
	{
		[SerializeField] private MediaContentController _contentController;
		[SerializeField] private Button _settingsButton, _muteButton;

		public void Init(GameObject mediaPrefab, ICommonFactory factory, Action showServerPopup)
		{
			_settingsButton.onClick.AddListener(() =>
			{
				SetUiBlocker(true);

				showServerPopup?.Invoke();
			});

			_muteButton.onClick.AddListener(LocalNetworkClient.SendMuteMessage);

			InitVersionTitle();

			void OnMediaInfoReceived(Dictionary<int, string> dictionary)
			{
#if UNITY_ANDROID
				_contentController.Init(factory, mediaPrefab, LocalNetworkClient.SendPlayMessage, dictionary);
#endif
				LocalNetworkClient.OnMediaInfoReceived -= OnMediaInfoReceived;
			}

			LocalNetworkClient.OnMediaInfoReceived += OnMediaInfoReceived;
			LocalNetworkClient.OnThumbnailReceived += OnThumbnailReceived;

			_contentController.OnThumbnailsLoaded += AllThumbnailsLoaded;
		}

		private void OnThumbnailReceived(Texture2D texture)
		{
#if UNITY_ANDROID
			_contentController.SetThumbnail(texture);
#endif
		}

		private void AllThumbnailsLoaded()
		{
			LocalNetworkClient.OnThumbnailReceived -= OnThumbnailReceived;
			_contentController.OnThumbnailsLoaded -= AllThumbnailsLoaded;

			SetUiBlocker(false);

			Debug.Log("all thumbnails loaded");
		}

		private void OnDestroy()
		{
			_settingsButton.onClick.RemoveAllListeners();
			_muteButton.onClick.RemoveAllListeners();
		}
	}
}