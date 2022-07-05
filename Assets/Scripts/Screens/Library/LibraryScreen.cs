﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Configs;
using Core;
using UnityEngine;
using UnityEngine.UI;
using VideoPlaying;

namespace Library
{
	public class LibraryScreen : MonoBehaviour
	{
		[SerializeField] private Button _saveButton, _exitButton;
		[SerializeField] private GameObject _exampleFile;
		[SerializeField] private RectTransform _contentHolder;
		[SerializeField] private Scrollbar _scrollbar;

		private bool _isExtensionsVisible;
		private Action _quitButtonAction;
		private LibraryFile[] _files;
		private MediaConfig _config;

		public void Init(ICommonFactory factory, MediaConfig config, Action quitButtonAction)
		{
			_quitButtonAction = quitButtonAction;
			_config = config;

			gameObject.SetActive(true);

			InitButtons();

			_scrollbar.value = Constants.ScrollbarDefaultValue;

			InitMediaItems(config, factory);
		}

		private void InitMediaItems(MediaConfig config, ICommonFactory commonFactory)
		{
			_files = new LibraryFile[config.MediaFiles.Length];

			for (var i = 0; i < config.MediaFiles.Length; i++)
			{
				var libraryItemInstance = commonFactory.InstantiateObject<LibraryFile>(_exampleFile, _contentHolder);
				libraryItemInstance.name = i.ToString();
				libraryItemInstance.Init(OnColorClicked);
				libraryItemInstance.SetFileName(Path.GetFileNameWithoutExtension(config.MediaFiles[i].name));
				libraryItemInstance.SetColorText(Settings.videoColor[Settings.library[i]], Settings.colorDefaults
					.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.library[i]]).Value);
				libraryItemInstance.SetParent(_contentHolder.transform);

				_files[i] = libraryItemInstance;
			}
		}


		private void ExitButtonClicked()
		{
			_saveButton.onClick.RemoveAllListeners();
			_exitButton.onClick.RemoveAllListeners();

			foreach (var libraryFile in _files)
				libraryFile.Close();

			_files = null;

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
			_isExtensionsVisible = !_isExtensionsVisible;

			//for (var i = 0; i < _config.MediaFiles.Length; i++)
			//{
			//	var file = _files[i];
			//	var title = _isExtensionsVisible ? ;

			//	file.SetFileName(title);
			//}
		}

		private void OnColorClicked(GameObject clickedObject, bool isNextColor)
		{
			if(isNextColor)
				NextColorClicked(clickedObject);
			else 
				PreviousColorClicked(clickedObject);
		}

		public void NextColorClicked(GameObject callingObj)
		{
			var index = int.Parse(callingObj.name);
			var libFile = callingObj.transform.parent.GetComponent<LibraryFile>();
		
			ChangeColor(index, libFile, true);
		}

		public void PreviousColorClicked(GameObject callingObj)
		{
			var index = int.Parse(callingObj.name);
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

		private static void ChangeColor(int index, LibraryFile libFile, bool next)
		{
			Settings.videoColor[Settings.library[index]] = Settings
				.colorDefaults[
					SRSUtilities.Wrap(
						Settings.colorDefaults.IndexOfFirstMatch(cd =>
							cd.Key == Settings.videoColor[Settings.library[index]]) + (next ? 1 : -1),
						Settings.colorDefaults.Length)].Key;
			libFile.SetColorText(Settings.videoColor[Settings.library[index]], Settings.colorDefaults
				.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.library[index]]).Value);
		}
	}
}
