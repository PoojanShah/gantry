using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VideoPlaying
{
	public class VideoItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text _title;
		[SerializeField] private Button _button;

		private int _id;
		private Action<int> _onClick;

		public void Init(int id, Action<int> onClickAction, string videoTitle)
		{
			_id = id;
			_onClick = onClickAction;

			_title.text = videoTitle;

			_button.onClick.AddListener(ItemClicked);
		}

		private void ItemClicked()
		{
			_onClick?.Invoke(_id);

			_button.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}
