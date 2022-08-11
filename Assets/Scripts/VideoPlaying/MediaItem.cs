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

			_title.text = content.Name.Split(Constants.Dot)[0];

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(ItemClicked);
		}

		public void ItemClicked() => _onClick?.Invoke(_content.Id);
		public void SetInteractable(bool isInteractable) => _button.interactable = isInteractable;
		private void OnDestroy() => _button.onClick.RemoveAllListeners();
	}
}
