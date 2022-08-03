using System;
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
		[SerializeField] private Button _patternToLoadPrefab, _cancelButton;
		[SerializeField] private Transform _patternsToLoadParent;

		private string[] _files;
		private ICommonFactory _commonFactory;
		private Action _onCloseAction;

		public void Init(ICommonFactory commonFactory, Action onCloseAction)
		{
			_onCloseAction = onCloseAction;
			_commonFactory = commonFactory;
			
			_cancelButton.onClick.AddListener(Clear);

			_files = Directory.GetFiles(Settings.GantryPatternsPath, Constants.GantrySearchPattern);
			
			for (var i = 0; i < _files.Length; i++)
			{
				var f = _files[i];
				var file = Path.GetFileName(f);
				var button = _commonFactory.InstantiateObject<Button>(_patternToLoadPrefab.gameObject, _patternsToLoadParent);
				var id = i;

				button.onClick.AddListener(() => 
					ChooseFileButtonAction(id));

				var text = button.GetComponentInChildren<TextMeshProUGUI>();
				text.text = file;
			}

			ContourEditor.HideGUI = true;
		}

		private void ChooseFileButtonAction(int i)
		{
			ContourEditor.LoadConfigurationByName(_files[i]);

			Clear();
		}

		private void Clear()
		{
			_cancelButton.onClick.RemoveAllListeners();

			_onCloseAction?.Invoke();

			ContourEditor.HideGUI = false;
			
			Destroy(gameObject);
		}
	}
}