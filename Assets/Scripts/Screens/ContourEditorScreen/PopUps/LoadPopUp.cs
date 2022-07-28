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

		public void Init(ICommonFactory commonFactory)
		{
			_commonFactory = commonFactory;
			
			_cancelButton.onClick.AddListener(() => Destroy(gameObject));

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
		}

		private void ChooseFileButtonAction(int i)
		{
			ContourEditor.LoadConfigurationByName(_files[i]);
			
			_cancelButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}