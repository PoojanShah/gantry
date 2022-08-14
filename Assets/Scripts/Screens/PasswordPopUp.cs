using System;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class PasswordPopUp : MonoBehaviour
	{
		private const string ADMIN_PASSWORD_TEXT = "Please enter the administrator password:";

		[SerializeField] private Button _okButton, _cancelButton;
		[SerializeField] private TMP_InputField _inputField;
		[SerializeField] private TMP_Text _descriptionText;
		
		public void Init(Action<string> onOkButtonClick, Action onCancelAction)
		{
			_cancelButton.onClick.AddListener(() =>
			{
				onCancelAction?.Invoke();

				Close();
			});

			_okButton.onClick.AddListener(() => onOkButtonClick.Invoke(_inputField.text));

#if UNITY_EDITOR
			_inputField.text = Constants.CorrectAdminPassword;
#endif

			_descriptionText.text = ADMIN_PASSWORD_TEXT;
		}

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.Return))
				_okButton.onClick.Invoke();
		}

		private void Close()
		{
			_okButton.onClick.RemoveAllListeners();
			_cancelButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}