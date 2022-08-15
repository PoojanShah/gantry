using System;
using UnityEngine;
using System.Collections.Generic;
using Core;
using Network;
using UnityEngine.UI;

namespace Screens
{
	public class MainMenuAndroid : MonoBehaviour
	{
		[SerializeField] private MediaContentController _contentController;
		[SerializeField] private Button _settingsButton;

		public void Init(GameObject mediaPrefab, ICommonFactory factory, Action showServerPopup)
		{
			_settingsButton.onClick.AddListener(() => showServerPopup?.Invoke());

			void OnMediaInfoReceived(Dictionary<int, string> dictionary)
			{
#if UNITY_ANDROID
				_contentController.Init(factory, mediaPrefab, SendPlayVideoCommand, dictionary);
#endif
				LocalNetworkClient.OnMediaInfoReceived -= OnMediaInfoReceived;
			}

			LocalNetworkClient.OnMediaInfoReceived += OnMediaInfoReceived;
		}

		private void OnDestroy() => _settingsButton.onClick.RemoveAllListeners();
		private static void SendPlayVideoCommand(int videoId) =>
			LocalNetworkClient.SendPlayMessage(NetworkHelper.LastIpNumber, videoId);
	}
}