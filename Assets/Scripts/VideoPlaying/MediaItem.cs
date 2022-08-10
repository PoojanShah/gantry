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
		private Action<int> _onClick;

#if UNITY_STANDALONE_WIN
		public void Init(MediaContent content, Action<int> onClickAction)
		{
			_content = content;
			_onClick = onClickAction;

			_title.text = content.Name;

			_button.onClick.AddListener(ItemClicked);
		}

		public void ItemClicked() => _onClick?.Invoke(_content.Id);
#elif UNITY_ANDROID
		public void Init(int id, Action<int> onClickAction)
		{
			_onClick = onClickAction;

			_title.text = id.ToString();

			_button.onClick.AddListener(ItemClicked);
		}
#endif
		public void SetInteractable(bool isInteractable) => _button.interactable = isInteractable;
		private void OnDestroy() => _button.onClick.RemoveAllListeners();
	}
}
