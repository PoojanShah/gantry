using System;
using Media;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VideoPlaying
{
	public class MediaItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text _title;
		[SerializeField] private Button _button;

		private MediaContent _content;
#if UNITY_STANDALONE_WIN
		private Action<MediaContent> _onClick;
#elif UNITY_ANDROID
		public int Id { get; private set; }

		private Action<int> _onClick;
#endif

#if UNITY_STANDALONE_WIN
		public void Init(MediaContent content, Action<MediaContent> onClickAction, string videoTitle)
		{
			_content = content;
			_onClick = onClickAction;

			_title.text = videoTitle;

			_button.onClick.AddListener(ItemClicked);
		}

		private void ItemClicked() => _onClick?.Invoke(_content);
#elif UNITY_ANDROID
		public void Init(int id, Action<int> onClickAction)
		{
			Id = id;
			_onClick = onClickAction;

			_title.text = id.ToString();

			_button.onClick.AddListener(ItemClicked);
		}

		private void ItemClicked() => _onClick?.Invoke(Id);
#endif
		public void SetInteractable(bool isInteractable) => _button.interactable = isInteractable;
		private void OnDestroy() => _button.onClick.RemoveAllListeners();
	}
}
