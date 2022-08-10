using UnityEngine;
using System.Collections.Generic;
using Core;
using Network;
using TMPro;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class MainMenuAndroid : MonoBehaviour
	{
		private readonly Vector2Int _ipValueRange = new(2, 255);

		[SerializeField] private Button _connectButton;
		[SerializeField] private TMP_InputField _ipEnd;
		[SerializeField] private TMP_Text _ipStart;
		[SerializeField] private MediaContentController _contentController;

		private List<MediaItem> _mediaItems;

		public void Init(GameObject mediaPrefab, ICommonFactory factory)
		{
			void OnMediaAmountReceived(int amount)
			{
#if UNITY_ANDROID
				_contentController.Init(factory, mediaPrefab, SendPlayVideoCommand, amount);
#endif
				LocalNetworkClient.OnMediaAmountReceived -= OnMediaAmountReceived;
			}

			LocalNetworkClient.OnMediaAmountReceived += OnMediaAmountReceived;

			_connectButton.onClick.AddListener(ConnectClicked);

			_ipEnd.onEndEdit.AddListener(VerifyIpNumber);

			InitIpLabel();
		}

		private void ConnectClicked()
		{
			if (!int.TryParse(_ipEnd.text, out var number))
				return;

			LocalNetworkClient.SendPlayMessage(number, -1);
		}

		private void SendPlayVideoCommand(int videoId)
		{
			if (!int.TryParse(_ipEnd.text, out var number)) 
				return;

			LocalNetworkClient.SendPlayMessage(number, videoId);
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
			_connectButton.onClick.RemoveAllListeners();
			_ipEnd.onEndEdit.RemoveAllListeners();
		}
	}
}