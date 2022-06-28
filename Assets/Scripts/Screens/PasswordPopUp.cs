using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public enum PasswordType : byte
	{
		Admin,
		SuperAdmin
	}
	
	public class PasswordPopUp : MonoBehaviour
	{
		private const string ADMIN_PASSWORD_TEXT = "Please enter the administrator password:";
		private const string SUPER_ADMIN_PASSWORD_TEXT = "Please enter the super administrator password:";
		[SerializeField] private Button _okButton, _cancelButton;
		[SerializeField] private InputField _inputField;
		[SerializeField] private Text _descriptionText;
		
		public void Init(Action<string> onOkButtonClick, PasswordType type)
		{
			_cancelButton.onClick.AddListener(Close);
			_okButton.onClick.AddListener(() => onOkButtonClick.Invoke(_inputField.text));
			
			SetTextByType(type);
		}

		private void SetTextByType(PasswordType type)
		{
			switch (type)
			{
				case PasswordType.Admin:
					_descriptionText.text = ADMIN_PASSWORD_TEXT;
					break;
				case PasswordType.SuperAdmin:
					_descriptionText.text = SUPER_ADMIN_PASSWORD_TEXT;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
		
		private void Close() => Destroy(gameObject);
	}
}