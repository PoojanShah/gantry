using System;
using Core;
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
		public void Init(MediaContent content, Action<int> onClickAction)
		{
			_content = content;
			_onClick = onClickAction;

#if UNITY_STANDALONE
			_title.text = content.Name.Split(Constants.Dot)[0];
#elif UNITY_ANDROID
			_title.text = string.IsNullOrEmpty(content.Name) ? content.Id.ToString() : content.Name;
#endif

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(ItemClicked);
		}

		public void UpdateTitle(string title)
		{
			_title.text = title;
			_content.Name = title;
		}

		public void ItemClicked() => _onClick?.Invoke(_content.Id);
		public void SetInteractable(bool isInteractable) => _button.interactable = isInteractable;
		private void OnDestroy() => _button.onClick.RemoveAllListeners();
	}
}
