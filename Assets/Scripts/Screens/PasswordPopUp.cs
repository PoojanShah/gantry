using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class PasswordPopUp : MonoBehaviour
	{
		[SerializeField] private Button _okButton, _cancelButton;
		[SerializeField] private InputField _inputField;

		public void Init(Action<string> onOkButtonClick)
		{
			_cancelButton.onClick.AddListener(Close);
			_okButton.onClick.AddListener(() => onOkButtonClick.Invoke(_inputField.text));
		}

		private void Close() => Destroy(gameObject);
	}
}