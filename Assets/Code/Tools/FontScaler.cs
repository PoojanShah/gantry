using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tools
{
	public class FontScaler : MonoBehaviour
	{
		[SerializeField] private float _defaultHeight = 60.97894f;
		[SerializeField] private float _defaultFontSize = 12;
		[SerializeField] private Text[] _childTextObjects;

		private Text _text;
		private float _lastHeight;
		private RectTransform _rectTransform;

		private void Awake()
		{
			_text = GetComponent<Text>();
			_rectTransform = GetComponent<RectTransform>();
		}

		private void Update()
		{
			HeightSizeHandler();

			void HeightSizeHandler()
			{
				if (Math.Abs(_rectTransform.rect.height - _lastHeight) < float.Epsilon)
					return;

				var ratio = _rectTransform.rect.height / _defaultHeight;

				_text.fontSize = (int)(_defaultFontSize * ratio);

				_lastHeight = _rectTransform.rect.height;

				foreach (var textObj in _childTextObjects)
					textObj.fontSize = _text.fontSize;
			}
		}
	}
}