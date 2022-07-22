using System.Collections.Generic;
using System.IO;
using ContourEditorTool;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen.PopUps
{
	public class LoadPopUp : MonoBehaviour
	{
		[SerializeField] private Button _buttonPrefab, _cancelButton;
		[SerializeField] private Transform _buttonsHolder;

		private string[] _files;
		private List<Button> _buttons;
		private ICommonFactory _commonFactory;

		public void Init(ICommonFactory commonFactory)
		{
			_commonFactory = commonFactory;
			
			_cancelButton.onClick.AddListener(() => Destroy(gameObject));

			_files = Directory.GetFiles(Settings.GantryPatternsPath, Constants.GantryExtension);
			
			_buttons = new List<Button>();

			for (var i = 0; i < _files.Length; i++)
			{
				var f = _files[i];
				var file = Path.GetFileName(f);

				var button = _commonFactory.InstantiateObject<Button>(_buttonPrefab.gameObject, _buttonsHolder);

				var currentID = i;
				button.onClick.AddListener(() => 
					ChooseFileButtonAction(currentID));

				var text = button.GetComponentInChildren<TextMeshProUGUI>();
				text.text = file;
				
				_buttons.Add(button);
			}
		}

		private void ChooseFileButtonAction(int i)
		{
			ContourEditor.LoadConfigurationByName(_files[i]);
			Destroy(gameObject);
		}
	}
}