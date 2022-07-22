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

		public void Start()
		{
			_saveButton.onClick.AddListener(() =>
			{
				ContourEditor.SaveConfiguration(_inputField.text);
				gameObject.SetActive(false);
			});
			
			_cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
		}
	}
}