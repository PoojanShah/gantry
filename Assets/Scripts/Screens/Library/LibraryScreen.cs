using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Library
{
	public class LibraryScreen : MonoBehaviour
	{
		private readonly LinkedList<LibraryFile> _libFiles = new();

		[SerializeField] private Button _saveButton, _exitButton;
		[SerializeField] private Image _exampleFile;
		[SerializeField] private RectTransform _contentHolder;
		[SerializeField] private Scrollbar _scrollbar;

		private bool _showFileExtensions;
		private Action _quitButtonAction;

		public void Init(Action quitButtonAction)
		{
			_quitButtonAction = quitButtonAction;

			gameObject.SetActive(true);

			InitButtons();
		
			LibraryReloadHandler();

			_scrollbar.value = Constants.ScrollbarDefaultValue;
		}

		private void ExitButtonClicked()
		{
			_saveButton.onClick.RemoveAllListeners();
			_exitButton.onClick.RemoveAllListeners();

			foreach (var libraryFile in _libFiles)
				libraryFile.Close();

			_libFiles.Clear();

			gameObject.SetActive(false);

			_quitButtonAction?.Invoke();
		}

		private void InitButtons()
		{
			_saveButton.onClick.AddListener(SaveButtonClicked);
			_exitButton.onClick.AddListener(ExitButtonClicked);
		}

		public void ShowFileExtensions()
		{
			_showFileExtensions = !_showFileExtensions;

			var count = 0;

			foreach (var file in _libFiles)
			{
				file.FileNameText.text = _showFileExtensions
					? Settings.library[count]
					: Path.GetFileNameWithoutExtension(Settings.library[count]);

				count++;
			}
		}


		public void NextColorClicked(GameObject callingObj)
		{
			var index = int.Parse(callingObj.transform.parent.name);
			var libFile = callingObj.transform.parent.GetComponent<LibraryFile>();
		
			ChangeColor(index, libFile, true);
		}

		public void PrevColorClicked(GameObject callingObj)
		{
			var index = int.Parse(callingObj.transform.parent.name);
			var libFile = callingObj.transform.parent.GetComponent<LibraryFile>();

			ChangeColor(index, libFile, false);
		}

		public void SaveButtonClicked()
		{
			try
			{
				var sw = new StreamWriter(Settings.movieColorFile);
				foreach (KeyValuePair<string, string> kvp in Settings.videoColor)
					sw.WriteLine(kvp.Key + Core.Constants.Colon + kvp.Value);
				sw.Close();
			}
			catch (Exception e)
			{
				Debug.LogError("Error writing file " + Settings.movieColorFile + Core.Constants.Colon + e);
			}

			_quitButtonAction?.Invoke();

			gameObject.SetActive(false);
		}

		private void LibraryReloadHandler()
		{
			if (_libFiles.Count > 0)
			{
				_libFiles.RemoveFirst();

				const int minAllowedLibrariesAmount = 1;

				while (_libFiles.Count > minAllowedLibrariesAmount)
				{
					var fileGObject = _libFiles.Last.Value.gameObject;

					Destroy(fileGObject);

					_libFiles.RemoveLast();
				}
			}

			var genNewFiles = Settings.library.Length;

			if (genNewFiles == 0)
			{
				_exampleFile.gameObject.SetActive(false);

				return;
			}

			for (var i = 0; i < genNewFiles; i++)
			{
				var clone = Instantiate(_exampleFile).GetComponent<LibraryFile>();
				clone.gameObject.SetActive(true);

				clone.name = i.ToString();
				_libFiles.AddLast(clone);

				try
				{
					clone.FileNameText.text = Path.GetFileNameWithoutExtension(Settings.library[i]);
					clone.ColorText.text = Settings.videoColor[Settings.library[i]];
					clone.ColorText.color = Settings.colorDefaults
						.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.library[i]]).Value;
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}

				clone.Init(i);
				clone.transform.SetParent(_contentHolder.transform);
				clone.transform.localScale = Vector3.one;
			}
		}

		public void ShowLibraryOptions()
		{
			if (_libFiles.Count > 0 && _libFiles.Count == Settings.library.Length)
			{
				var count = 0;

				foreach (var libFile in _libFiles)
				{
					if ((_showFileExtensions
						    ? Settings.library[count]
						    : Path.GetFileNameWithoutExtension(Settings.library[count]))
					    .Equals(libFile.FileNameText.text))
					{
						continue;
					}

					count++;
				}
			}
		}

		private static void ChangeColor(int index, LibraryFile libFile, bool next)
		{
			Settings.videoColor[Settings.library[index]] = Settings
				.colorDefaults[
					SRSUtilities.Wrap(
						Settings.colorDefaults.IndexOfFirstMatch(cd =>
							cd.Key == Settings.videoColor[Settings.library[index]]) + (next ? 1 : -1),
						Settings.colorDefaults.Length)].Key;
			libFile.ColorText.text = Settings.videoColor[Settings.library[index]];
			libFile.ColorText.color = Settings.colorDefaults
				.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.library[index]]).Value;
		}
	}
}
