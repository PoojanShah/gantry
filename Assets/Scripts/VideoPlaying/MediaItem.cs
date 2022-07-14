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
		private Action<MediaContent> _onClick;

		public void Init(MediaContent content, Action<MediaContent> onClickAction, string videoTitle)
		{
			_content = content;
			_onClick = onClickAction;

			_title.text = videoTitle;

			_button.onClick.AddListener(ItemClicked);
		}

		private void ItemClicked()
		{
			_onClick?.Invoke(_content);

			_button.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}
