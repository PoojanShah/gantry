using System;
using System.Collections.Generic;
using System.IO;
using ContourEditorTool;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen
{
	public class LoadPopUp : MonoBehaviour
	{
		[SerializeField] private Button _buttonPrefab, _cancelButton;
		[SerializeField] private Transform _buttonsHolder;

		private string[] _files;
		private List<Button> _buttons;

		public void Start()
		{
			_cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
		}

		private void OnEnable()
		{
			_buttons = new List<Button>();
			
			_files = Directory.GetFiles(Settings.GantryPatternsPath, Constants.GantryExtension);

			for (var i = 0; i < _files.Length; i++)
			{
				var f = _files[i];
				var file = Path.GetFileName(f);

				var buttonGO = Instantiate(_buttonPrefab.gameObject, _buttonsHolder);

				var button = buttonGO.GetComponent<Button>();

				var ii = i;
				button.onClick.AddListener(() =>
				{
					ContourEditor.LoadConfigurationByName(_files[ii]);
					gameObject.SetActive(false);
				});

				var text = button.GetComponentInChildren<TextMeshProUGUI>();
				text.text = file;
				
				_buttons.Add(button);
			}
		}

		private void OnDisable()
		{
			if (_buttons != null)
				for (int i = _buttons.Count - 1; i >= 0; i--)
				{
					Destroy(_buttons[i].gameObject);
				}

			_buttons?.Clear();
		}
	}
}