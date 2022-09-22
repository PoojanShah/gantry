using System;
using System.IO;
using System.Linq;
using ContourEditorTool;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen.PopUps
{
	public class SavePopUp : MonoBehaviour
	{
		[SerializeField] private TMP_InputField _inputField;
		[SerializeField] private Toggle _saveAsDefaultToggle, _saveAsDefaultWallToggle;
		[SerializeField] private Button _saveButton, _cancelButton;

		private Action _onClose;
		private Action _showOverwritePopup;
		private bool _isDuoMode;

		public void Init(Action onClose, Action showOverwritePopup, bool isDuoMode)
		{
			_onClose = onClose;
			_showOverwritePopup = showOverwritePopup;
			_isDuoMode = isDuoMode;

			_saveAsDefaultWallToggle.gameObject.SetActive(_isDuoMode);

			_saveButton.onClick.AddListener(SaveButtonAction);
			_cancelButton.onClick.AddListener(Clear);

			ContourEditor.HideGUI = true;
		}

		private void SaveButtonAction()
		{
			var files = Directory.GetFiles(Settings.GantryPatternsPath, Constants.GantrySearchPattern);

			if (files.Any(f => Path.GetFileNameWithoutExtension(f) == _inputField.text))
				_showOverwritePopup?.Invoke();
			else
				Save();
		}

		public void Save()
		{
			ContourEditor.SaveConfiguration(_inputField.text, _saveAsDefaultToggle.isOn,
				!_isDuoMode || _saveAsDefaultWallToggle.isOn);

			Clear();
		}

		private void Clear()
		{
			_cancelButton.onClick.RemoveAllListeners();

			_onClose?.Invoke();

			ContourEditor.HideGUI = false;
			
			Destroy(gameObject);
		}
	}
}