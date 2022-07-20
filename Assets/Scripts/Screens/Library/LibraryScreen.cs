using System;
using System.IO;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Library
{
	public class LibraryScreen : MonoBehaviour
	{
		[SerializeField] private Button _saveButton, _exitButton;
		[SerializeField] private Toggle _extensionToggle;
		[SerializeField] private GameObject _exampleFile;
		[SerializeField] private RectTransform _contentHolder;
		[SerializeField] private Scrollbar _scrollbar;

		private Action _quitButtonAction;
		private LibraryFile[] _files;

		public void Init(ICommonFactory factory, Action quitButtonAction)
		{
			_quitButtonAction = quitButtonAction;

			_extensionToggle.onValueChanged.AddListener(ShowFileExtensions);

			InitButtons();

			_scrollbar.value = Constants.ScrollbarDefaultValue;

			InitMediaItems(factory);
		}

		private void InitMediaItems(ICommonFactory commonFactory)
		{
			if (Settings.MediaLibrary == null || Settings.MediaLibrary.Length == 0)
				return;

			_files = new LibraryFile[Settings.MediaLibrary.Length];

			for (var i = 0; i < Settings.MediaLibrary.Length; i++)
			{
				var libraryItemInstance = commonFactory.InstantiateObject<LibraryFile>(_exampleFile, _contentHolder);
				libraryItemInstance.name = i.ToString();
				libraryItemInstance.Init(OnColorClicked);
				libraryItemInstance.SetFileName(Path.GetFileNameWithoutExtension(Settings.MediaLibrary[i]));
				libraryItemInstance.SetColorText(Settings.VideoColors[Settings.MediaLibrary[i]], Constants.colorDefaults
					.FirstOrDefault(cd => cd.Key == Settings.VideoColors[Settings.MediaLibrary[i]]).Value);
				libraryItemInstance.SetParent(_contentHolder.transform);

				_files[i] = libraryItemInstance;
			}
		}


		private void ExitButtonClicked()
		{
			_saveButton.onClick.RemoveAllListeners();
			_exitButton.onClick.RemoveAllListeners();
			_extensionToggle.onValueChanged.RemoveAllListeners();

			if (_files != null)
			{
				foreach (var libraryFile in _files)
					libraryFile.Close();

				_files = null;
			}

			gameObject.SetActive(false);

			_quitButtonAction?.Invoke();
		}

		private void InitButtons()
		{
			_saveButton.onClick.AddListener(SaveButtonClicked);
			_exitButton.onClick.AddListener(ExitButtonClicked);
		}

		public void ShowFileExtensions(bool isShown)
		{
			var libraryLength = Settings.MediaLibrary.Length;

			for (var i = 0; i < libraryLength; i++)
			{
				var file = _files[i];
				var title = isShown
					? Settings.MediaLibrary[i]
					: Path.GetFileNameWithoutExtension(Settings.MediaLibrary[i]);

				file.SetFileName(title);
			}
		}

		private void OnColorClicked(GameObject clickedObject, bool isNextColor)
		{
			if (isNextColor)
				NextColorClicked(clickedObject);
			else
				PreviousColorClicked(clickedObject);
		}

		public void NextColorClicked(GameObject callingObj)
		{
			var index = int.Parse(callingObj.name);
			var libFile = callingObj.GetComponent<LibraryFile>();

			ChangeColor(index, libFile, true);
		}

		public void PreviousColorClicked(GameObject callingObj)
		{
			var index = int.Parse(callingObj.name);
			var libFile = callingObj.GetComponent<LibraryFile>();

			ChangeColor(index, libFile, false);
		}

		public void SaveButtonClicked()
		{
			try
			{
				var sw = new StreamWriter(Settings.ColorsConfigPath);

				foreach (var kvp in Settings.VideoColors)
					sw.WriteLine(kvp.Key + Core.Constants.Colon + kvp.Value);

				sw.Close();
			}
			catch (Exception e)
			{
				Debug.LogError("Error writing file " + Settings.ColorsConfigPath + Core.Constants.Colon + e);
			}

			_quitButtonAction?.Invoke();

			gameObject.SetActive(false);
		}

		private static void ChangeColor(int index, LibraryFile libFile, bool next)
		{
			Settings.VideoColors[Settings.MediaLibrary[index]] = Constants
				.colorDefaults[
					SRSUtilities.Wrap(
						Constants.colorDefaults.IndexOfFirstMatch(cd =>
							cd.Key == Settings.VideoColors[Settings.MediaLibrary[index]]) + (next ? 1 : -1),
						Constants.colorDefaults.Length)].Key;

			var colorTitle = Settings.VideoColors[Settings.MediaLibrary[index]];
			var color = Constants.colorDefaults
				.FirstOrDefault(cd => cd.Key == Settings.VideoColors[Settings.MediaLibrary[index]]).Value;

			libFile.SetColorText(colorTitle, color);
		}
	}
}
