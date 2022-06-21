using System;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class PasswordPopUp : MonoBehaviour
	{
		[SerializeField] private Button _okButton, _cancelButton;
		[SerializeField] private InputField _inputField;
		[SerializeField] private Text _descriptionText;

		private const string AdminPassText = "Please enter the administrator password:";
		private const string SuperPassText = "Please enter the super administrator password:";
		
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
					_descriptionText.text = AdminPassText;
					break;
				case PasswordType.SuperAdmin:
					_descriptionText.text = SuperPassText;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
		
		private void Close() => Destroy(gameObject);
	}

	public enum PasswordType
	{
		Admin,
		SuperAdmin
	}

	public static class LogIn
	{
		private static bool _adminIsLogIn = false;
		private static bool _superAdminIsLogIn = false;
		
		public static void LogInByType(PasswordType type)
		{
			switch (type)
			{
				case PasswordType.Admin:
					_adminIsLogIn = true;
					break;
				case PasswordType.SuperAdmin:
					_superAdminIsLogIn = true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public static bool CheckIsLogInByType(PasswordType type)
		{
			return type switch
			{
				PasswordType.Admin => _adminIsLogIn,
				PasswordType.SuperAdmin => _superAdminIsLogIn,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
		}

		public static string GetPasswordByType(PasswordType type)
		{
			return type switch
			{
				PasswordType.Admin => Constants.CorrectAdminPass,
				PasswordType.SuperAdmin => Constants.CorrectSuperPass,
				_ => null
			};
		}
	}
}