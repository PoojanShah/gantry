using System;
using UnityEngine;
using UnityEngine.UI;

namespace Library
{
	public class LibraryFile : MonoBehaviour
	{
		[SerializeField] private Transform _transform;
		[SerializeField] private Image _thumbnailImage;
		[SerializeField] private Text _fileNameText, _colorText;
		[SerializeField] private Button _nextColorButton, _previousColorButton;

		public void Init(Action<GameObject, bool> onColorButtonPressed)
		{
			_nextColorButton.onClick.AddListener(() => onColorButtonPressed?.Invoke(gameObject, true));
			_previousColorButton.onClick.AddListener(() => onColorButtonPressed?.Invoke(gameObject, false));
		}

		public void SetFileName(string fileName) => _fileNameText.text = fileName;

		public void SetParent(Transform parent)
		{
			_transform.SetParent(parent);
			_transform.localScale = Vector3.one;
		}

		public void SetColorText(string text, Color color)
		{
			_colorText.text = text;
			_colorText.color = color;
		}

		public void Close()
		{
			_nextColorButton.onClick.RemoveAllListeners();
			_previousColorButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}
