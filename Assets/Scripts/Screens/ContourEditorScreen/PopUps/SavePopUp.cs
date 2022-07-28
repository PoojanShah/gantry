using System;
using ContourEditorTool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen.PopUps
{
	public class SavePopUp : MonoBehaviour
	{
		[SerializeField] private TMP_InputField _inputField;
		[SerializeField] private Button _saveButton, _cancelButton;

		private Action _onClose;

		public void Init(Action onClose)
		{
			_onClose = onClose;

			_saveButton.onClick.AddListener(SaveButtonAction);
			_cancelButton.onClick.AddListener(Clear);
		}

		private void SaveButtonAction()
		{
			ContourEditor.SaveConfiguration(_inputField.text);

			Clear();
		}

		private void Clear()
		{
			_cancelButton.onClick.RemoveAllListeners();

			_onClose?.Invoke();

			Destroy(gameObject);
		}
	}
}