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
		[SerializeField] private RawImage _thumbnail;
		[SerializeField] private Sprite _defaultThumbnail;

		private MediaContent _content;
		private Action<int> _onClick;
		private bool _isThumbnailReplaced;

		public void Init(MediaContent content, Action<int> onClickAction, Texture2D thumbnail)
		{
			_content = content;
			_onClick = onClickAction;

			SetThumbnail(thumbnail);

			_title.text = content.Name.Split(Constants.Dot)[0];

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(ItemClicked);
		}

		public void SetThumbnail(Texture thumbnail)
		{
			if (thumbnail != null)
			{
				_thumbnail.texture = thumbnail;

				_isThumbnailReplaced = true;
			}
			else
			{
				if (_isThumbnailReplaced)
				{
					_isThumbnailReplaced = false;

					Destroy(_thumbnail.texture);

					_thumbnail.texture = _defaultThumbnail.texture;
				}
			}
		}

		public void ItemClicked() => _onClick?.Invoke(_content.Id);
		public void SetInteractable(bool isInteractable) => _button.interactable = isInteractable;
		private void OnDestroy() => _button.onClick.RemoveAllListeners();
	}
}
