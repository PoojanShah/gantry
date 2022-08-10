using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Library
{
	public class LibraryFile : MonoBehaviour
	{
		[SerializeField] private Transform _transform;
		[SerializeField] private Image _thumbnailImage, _colorTextBackground;
		[SerializeField] private TMP_Text _fileNameText, _colorText;
		[SerializeField] private Button _nextColorButton, _previousColorButton;
		[Range(0, 1)]
		[SerializeField] private float _colorBackgroundAlpha;
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
			color.a = _colorBackgroundAlpha; 
			_colorTextBackground.color = color;
		}

		public void Close()
		{
			_nextColorButton.onClick.RemoveAllListeners();
			_previousColorButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}
