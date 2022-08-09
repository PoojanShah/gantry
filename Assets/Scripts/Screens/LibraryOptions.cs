using System;
using Core;
using Library;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	//TODO: Change name when screen will have name
	public class LibraryOptions : MonoBehaviour
	{
		[SerializeField] private OptionsMenu _optionsMenu;
		[SerializeField] private LibraryScreen _libraryScreen;
		[SerializeField] private Button _exitButton;
		
		public void Init(ICommonFactory factory, Action quitAction)
		{
			_optionsMenu.Init();
			_libraryScreen.Init(factory);
			
			_exitButton.onClick.AddListener(() => Quit(quitAction));
		}

		private void Quit(Action quitAction)
		{
			_optionsMenu.SaveAndExit();
			_libraryScreen.SaveAndExit();
			
			_exitButton.onClick.RemoveAllListeners();
			
			quitAction?.Invoke();
			
			Destroy(gameObject);
		}
	}
}