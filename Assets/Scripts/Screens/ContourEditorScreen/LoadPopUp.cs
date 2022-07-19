using System;
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
		[SerializeField] private TMP_Dropdown _dropdownField;
		[SerializeField] private Button _loadButton;
		[SerializeField] private Button _cancelButton;

		private string[] files;
		
		public void Start()
		{
			_loadButton.onClick.AddListener(() =>
			{
				ContourEditor.LoadConfigurationByName(files[_dropdownField.value]);
				gameObject.SetActive(false);
			});
			
			_cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
		}

		private void OnEnable()
		{
			files = Directory.GetFiles(Settings.dataPath, Constants.GantryExtension);

			_dropdownField.options.Clear();
			foreach (var file in files)
			{
				_dropdownField.options.Add(new TMP_Dropdown.OptionData(file));
			}
		}
	}
}