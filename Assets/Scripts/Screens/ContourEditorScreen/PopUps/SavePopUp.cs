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

		public void Init()
		{
			_saveButton.onClick.AddListener(SaveButtonAction);
			_cancelButton.onClick.AddListener(() => Destroy(gameObject));
		}

		private void SaveButtonAction()
		{
			ContourEditor.SaveConfiguration(_inputField.text);

			_cancelButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}