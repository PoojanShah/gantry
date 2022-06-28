using System;
using UnityEngine;
using UnityEngine.UI;

namespace Library
{
	public class LibraryFile : MonoBehaviour
	{
		public Image ThumbnailImage;
		public Text FileNameText, ColorText;
		public Toggle BoyToggle, GirlToggle, ManToggle, WomanToggle;
		public Button LeftButton, RightButton;

		private int _id;
		private Action<int, Category, bool> _onToggleChanged;

		public void Init(int id, Action<int, Category, bool> onToggleChanged)
		{
			_id = id;
			_onToggleChanged = onToggleChanged;

			BoyToggle.onValueChanged.AddListener(isOn => ToggleChanged(Category.Boy, isOn));
			GirlToggle.onValueChanged.AddListener(isOn => ToggleChanged(Category.Girl, isOn));
			ManToggle.onValueChanged.AddListener(isOn => ToggleChanged(Category.Man, isOn));
			WomanToggle.onValueChanged.AddListener(isOn => ToggleChanged(Category.Woman, isOn));
		}

		private void ToggleChanged(Category category, bool isOn) => _onToggleChanged?.Invoke(_id, category, isOn);

		public void Close()
		{
			BoyToggle.onValueChanged.RemoveAllListeners();
			GirlToggle.onValueChanged.RemoveAllListeners();
			ManToggle.onValueChanged.RemoveAllListeners();
			WomanToggle.onValueChanged.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}
