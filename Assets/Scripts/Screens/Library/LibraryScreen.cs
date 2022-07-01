using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Library
{
	public enum Category : byte
	{
		Boy,
		Girl,
		Man,
		Woman
	}

	public class LibraryScreen : MonoBehaviour
	{
		private readonly LinkedList<LibraryFile> _libFiles = new();

		[SerializeField] private Button _allButton, _noneButton, _saveButton, _exitButton;
		[SerializeField] private Image _exampleFile;
		[SerializeField] private Text _boy, _girl, _man, _woman;
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
			_allButton.onClick.RemoveAllListeners();
			_noneButton.onClick.RemoveAllListeners();
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
			_allButton.onClick.AddListener(AllButtonClicked);
			_noneButton.onClick.AddListener(NoneButtonClicked);
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

		public void AllButtonClicked()
		{
			foreach (var file in _libFiles)
				file.BoyToggle.isOn = file.GirlToggle.isOn = file.ManToggle.isOn = file.WomanToggle.isOn = true;

			UpdateToggleAmounts();
		}

		public void NoneButtonClicked()
		{
			foreach (var file in _libFiles)
				file.BoyToggle.isOn = file.GirlToggle.isOn = file.ManToggle.isOn = file.WomanToggle.isOn = false;

			UpdateToggleAmounts();
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

		public void ToggleChanged(int id, Category category, bool isToggleOn)
		{
			var catIndex = (int)category;
			var libIndex = id;

			if (isToggleOn)
				if (!Settings.categories[catIndex].Contains(Settings.library[libIndex]))
					Settings.categories[catIndex].Concat(new[] { Settings.library[libIndex] });
				else if (Settings.categories[catIndex].Contains(Settings.library[libIndex]))
					Settings.categories[catIndex] =
						Settings.categories[catIndex].Where(w => w != Settings.library[libIndex]).ToArray();

			UpdateToggleAmounts();
		}

		public void SaveButtonClicked()
		{
			UpdateCategoryData();

			StreamWriter sw;

			try
			{
				sw = new StreamWriter(Settings.categoryFile);

				foreach (var category in Settings.categories)
				{
					var crossChecked = new List<string>();

					foreach (var c in category)
						if (Settings.library.Contains(c))
							crossChecked.Add(c);

					sw.WriteLine(string.Join(Core.Constants.Comma, crossChecked.ToArray()));
				}

				sw.Close();
			}
			catch (Exception e)
			{
				Debug.LogError("Error writing file " + Settings.categoryFile + Core.Constants.Colon + e);
			}

			try
			{
				sw = new StreamWriter(Settings.movieColorFile);
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
					clone.BoyToggle.isOn = Settings.categories[0].Contains(Settings.library[i]);
					clone.GirlToggle.isOn = Settings.categories[1].Contains(Settings.library[i]);
					clone.ManToggle.isOn = Settings.categories[2].Contains(Settings.library[i]);
					clone.WomanToggle.isOn = Settings.categories[3].Contains(Settings.library[i]);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}

				clone.Init(i, ToggleChanged);
				clone.transform.SetParent(_contentHolder.transform);
				clone.transform.localScale = Vector3.one;
			}

			UpdateToggleAmounts();
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

		private void UpdateToggleAmounts()
		{
			int boy = 0, girl = 0, man = 0, woman = 0;

			foreach (var file in _libFiles)
			{
				if (file.BoyToggle.isOn)
					boy++;

				if (file.GirlToggle.isOn)
					girl++;

				if (file.ManToggle.isOn)
					man++;

				if (file.WomanToggle.isOn)
					woman++;
			}

			_boy.text = $"{Core.Constants.BoyHash}\n{Core.Constants.OpeningBracket}{boy}{Core.Constants.ClosingBracket}";
			_girl.text = $"{Core.Constants.GirlHash}\n{Core.Constants.OpeningBracket}{girl}{Core.Constants.ClosingBracket}";
			_man.text = $"{Core.Constants.ManHash}\n{Core.Constants.OpeningBracket}{man}{Core.Constants.ClosingBracket}";
			_woman.text = $"{Core.Constants.WomanHash}\n{Core.Constants.OpeningBracket}{woman}{Core.Constants.ClosingBracket}";
		}

		private void UpdateCategoryData()
		{
			var boy = new LinkedList<string>();
			var girl = new LinkedList<string>();
			var man = new LinkedList<string>();
			var woman = new LinkedList<string>();
			var count = 0;

			foreach (var file in _libFiles)
			{
				if (file.BoyToggle.isOn)
					boy.AddLast(Settings.library[count]);
				if (file.GirlToggle.isOn)
					girl.AddLast(Settings.library[count]);
				if (file.ManToggle.isOn)
					man.AddLast(Settings.library[count]);
				if (file.WomanToggle.isOn)
					woman.AddLast(Settings.library[count]);

				count++;
			}

			Settings.categories[0] = boy.ToArray();
			Settings.categories[1] = girl.ToArray();
			Settings.categories[2] = man.ToArray();
			Settings.categories[3] = woman.ToArray();
		}
	}
}
