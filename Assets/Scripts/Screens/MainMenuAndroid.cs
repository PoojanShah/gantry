using UnityEngine;
using System;
using System.Collections.Generic;
using Core;
using Media;
using Network;
using TMPro;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class MainMenuAndroid : MonoBehaviour
	{
		private readonly Vector2Int _ipValueRange = new(2, 255);

		[SerializeField] private Button _exitButton;
		[SerializeField] private Transform _parent;
		[SerializeField] private TMP_InputField _ipEnd;
		[SerializeField] private TMP_Text _ipStart;

		private List<MediaItem> _mediaItems;

		public void Init(Action onQuitAction, GameObject mediaPrefab, ICommonFactory factory)
		{
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			_ipEnd.onEndEdit.AddListener(VerifyIpNumber);

			InitIpLabel();

			InitMediaItems(factory, mediaPrefab);
		}

		private void SendPlayVideoCommand(int videoId)
		{
			if (!int.TryParse(_ipEnd.text, out var number)) 
				return;

			Debug.Log("start client");

			CustomNetworkClient.SendMessagePlay(number, videoId);
		}

		private void InitIpLabel() => _ipStart.text = NetworkHelper.GetMyIpWithoutLastNumberString();

		private void VerifyIpNumber(string currentValue)
		{
			if (int.TryParse(currentValue, out var number))
			{
				if (number < _ipValueRange.x)
					number = _ipValueRange.x;
				else if (number > _ipValueRange.y)
					number = _ipValueRange.y;

				Debug.Log("update ip");

				_ipEnd.text = number.ToString();
			}
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
			_ipEnd.onEndEdit.RemoveAllListeners();

			ClearMediaItems();
		}

		public void ClearMediaItems()
		{
			foreach (var mediaItem in _mediaItems)
				Destroy(mediaItem.gameObject);

			_mediaItems.Clear();
		}

		public void InitMediaItems(ICommonFactory commonFactory, GameObject mediaPrefab)
		{
#if UNITY_ANDROID
			const int mediaAmount = 12;

			_mediaItems = new List<MediaItem>();

			for (var i = 0; i < mediaAmount; i++)
			{
				var mediaItem = commonFactory.InstantiateObject<MediaItem>(mediaPrefab, _parent);
				mediaItem.Init(i, SendPlayVideoCommand);

				_mediaItems.Add(mediaItem);
			}

			SetMediaInteractable();

#endif
		}
	}
}